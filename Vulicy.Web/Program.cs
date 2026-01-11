using Vulicy.Services;
using Vulicy.Web.Endpoints;
using Vulicy.Web.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, VulicyJsonSerializerContext.Default);
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddConventionalServices(typeof(IImportingService).Assembly);

var app = builder.Build();

await app.Services.InitializeDatabases();

app.MapImport();
app.MapMap();

app.Run();
