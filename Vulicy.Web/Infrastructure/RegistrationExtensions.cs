using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vulicy.Services;

namespace Vulicy.Web.Infrastructure;

public static class RegistrationExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public void AddConventionalServices(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(t => !t.IsGenericType && !t.IsAbstract))
            foreach (var typeInterface in type.GetInterfaces().Where(i => i.Name == $"I{type.Name}"))
                serviceCollection.TryAddScoped(typeInterface, type);
        }

        public void AddConfigs(IConfiguration configuration, Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(x => x.Name.EndsWith("Config") && x is { IsClass: true, IsAbstract: false }))
            {
                var section = configuration.GetSection(type.Name);
                serviceCollection.AddSingleton(type, section.Get(type)!);
            }
        }
    }
}