using Vulicy.Services;
using Vulicy.Web.Endpoints;
using Vulicy.Web.Infrastructure;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, VulicyWebSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, VulicyServicesSerializerContext.Default);
});

builder.Services.AddConfigs(builder.Configuration, typeof(AuthConfig).Assembly);

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddConventionalServices(typeof(IImportingService).Assembly);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Vulicy.Auth";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

await app.Services.InitializeDatabases();

app.MapImport();
app.MapMap();
app.MapFeatures();
app.MapAuth();

app.MapFallbackToFile("index.html");

app.Run();

