using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Vulicy.Web.Infrastructure;

public static class RegistrationExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public void AddConventionalServices(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(t => !t.IsGenericType && !t.IsAbstract))
            foreach (var typeInterface in type.GetInterfaces().Where(i => i.Name == $"I{type.Name}"))
                serviceCollection.TryAddTransient(typeInterface, type);
        }
    }
}