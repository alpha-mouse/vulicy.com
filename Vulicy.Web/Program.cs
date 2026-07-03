using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Vulicy.Services;
using Vulicy.Web.Endpoints;
using Vulicy.Web.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateSlimBuilder(args);

var sentryDsn = builder.Configuration.GetValue<string>("AppConfig:SentryBeDsn");
if (!builder.Environment.IsDevelopment())
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = sentryDsn;
        o.Environment = builder.Environment.EnvironmentName.ToLowerInvariant();
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Clear();
    options.SerializerOptions.TypeInfoResolverChain.Add(VulicyWebSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(DiscourseJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(VulicyServicesSerializerContext.Default);

    // Add NTS GeoJSON converter for proper geometry serialization
    options.SerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddConfigs(builder.Configuration, typeof(DiscourseConfig).Assembly);
builder.Services.AddConfigs(builder.Configuration, typeof(FrontConfig).Assembly);

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddConventionalServices(typeof(IImportingService).Assembly);
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 2;

    // Comma-separated CIDRs of trusted proxies (Cloudflare ranges + the Coolify/Traefik network).
    // When empty (e.g. local dev) no forwarded headers are trusted and the scheme stays http.
    var knownNetworks = builder.Configuration.GetValue<string>("ForwardedHeaders:KnownNetworks");
    if (!string.IsNullOrWhiteSpace(knownNetworks))
        foreach (var cidr in knownNetworks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(cidr));
});

// Origin-side throttle (per client IP) so a single client cannot exhaust the DB pool on the
// anonymous, DB-heavy read endpoints. Cloudflare adds edge protection; this defends the origin.
var permitsPerMinute = builder.Configuration.GetValue<int?>("RateLimiting:PermitsPerMinute") ?? 600;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitsPerMinute,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });
});

builder.Services.AddAws(builder.Configuration);
builder.Services.AddSingleton<IAuditQueue, AuditQueue>();
builder.Services.AddHostedService<AuditPersistenceHostedService>();

builder.Services.AddSingleton<ILinksService, LinksService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Vulicy.Auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Force Secure in non-dev so the session cookie is never sent over the plaintext
        // proxy hop; left as default (SameAsRequest) in dev for http://localhost logins.
        if (!builder.Environment.IsDevelopment())
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationExtensions.RequireAdminPolicy, policy => policy.RequireRole(Auth.AdminRole));

builder.Services.AddValidatorsFromAssemblyContaining<FeatureEditRequest>();



var app = builder.Build();

var frontConfig = app.Services.GetRequiredService<FrontConfig>();
// dirty, but let it be so for now
frontConfig.DiscourseBaseUrl = app.Services.GetRequiredService<DiscourseConfig>().BaseUrl;
frontConfig.Environment = app.Environment.IsProduction() ? "production" : "development" ;

var sentryFeDsn = app.Configuration.GetValue<string>("AppConfig:SentryFeDsn");
var sentryFeOrigin = string.IsNullOrEmpty(sentryFeDsn) ? "" : $"https://{new Uri(sentryFeDsn).Host}";
var csp =
    "default-src 'none'; " +
    "script-src 'self'; " +
    "style-src 'self' 'unsafe-inline'; " +
    "font-src 'self'; " +
    "img-src 'self' data:; " +
    // MapLibre spawns web workers from blob: URLs; default-src 'none' would block them
    "worker-src blob:; " +
    // MapTiler serves style.json, vector tiles, glyphs and sprites; MapLibre fetches all via ajax (connect-src, not font-src/img-src)
    $"connect-src 'self' {sentryFeOrigin} https://api.maptiler.com; " +
    "base-uri 'self'; " +
    "form-action 'self'; " +
    "frame-ancestors 'none'";

// Must run before anything that reads Request.Scheme / RemoteIpAddress (security headers, auth,
// rate limiter, SSO callback URL construction).
app.UseForwardedHeaders();

var isDevelopment = app.Environment.IsDevelopment();
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=()");
    ctx.Response.Headers.Append("Content-Security-Policy", csp);
    // HSTS only in non-dev (never send it over plaintext localhost).
    if (!isDevelopment)
        ctx.Response.Headers.Append("Strict-Transport-Security", "max-age=15552000; includeSubDomains");
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseResponseCompression();

// After static files so SPA assets don't consume the per-IP budget; API + tile endpoints do.
app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<AuditMiddleware>(); // after authentication
app.UseAuthorization();

await app.Services.InitializeDatabases();

app.MapImport(withAdminAuth: !app.Environment.IsDevelopment());
app.MapMap();
app.MapFeatures();
app.MapDossierRecords();
app.MapAuth();
app.MapConfig();
app.MapForum();
app.MapAdministratives();

app.MapFallbackToFile("index.html");

app.Services.GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStarted.Register(() =>
    {
        app.Services.CheckValidators();
        app.Services.CheckNonGetEndpointsRequireAuthorization();
    });

await app.RunAsync();
