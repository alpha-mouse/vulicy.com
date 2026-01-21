using Vulicy.Services;
using Vulicy.Web.Endpoints;
using Vulicy.Web.Infrastructure;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, VulicyWebSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, DiscourseJsonSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(2, VulicyServicesSerializerContext.Default);
});

builder.Services.AddConfigs(builder.Configuration, typeof(DiscourseConfig).Assembly);
builder.Services.AddConfigs(builder.Configuration, typeof(FrontConfig).Assembly);

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddConventionalServices(typeof(IImportingService).Assembly);
builder.Services.AddHttpClient();

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

app.Run();

