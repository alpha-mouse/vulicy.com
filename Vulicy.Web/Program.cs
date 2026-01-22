using Vulicy.Services;
using Vulicy.Web.Endpoints;
using Vulicy.Web.Infrastructure;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Clear();
    options.SerializerOptions.TypeInfoResolverChain.Add(VulicyWebSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(DiscourseJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(VulicyServicesSerializerContext.Default);
});

builder.Services.AddConfigs(builder.Configuration, typeof(DiscourseConfig).Assembly);
builder.Services.AddConfigs(builder.Configuration, typeof(FrontConfig).Assembly);

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddConventionalServices(typeof(IImportingService).Assembly);
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Vulicy.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<FeatureEditRequest>();



var app = builder.Build();

app.Services.GetRequiredService<FrontConfig>().DiscourseBaseUrl = app.Services.GetRequiredService<DiscourseConfig>().BaseUrl; // dirty, but let it be so for now

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

await app.Services.InitializeDatabases();

app.MapImport();
app.MapMap();
app.MapFeatures();
app.MapDossierRecords();
app.MapAuth();
app.MapConfig();
app.MapForum();

app.MapFallbackToFile("index.html");

app.Services.GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStarted.Register(() => app.Services.CheckValidators());

await app.RunAsync();
