using System.Reflection;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
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

        public void AddAws(IConfiguration configuration)
        {
            var settings = configuration.GetSection("AwsConfig").Get<AwsConfig>();
            var region = RegionEndpoint.GetBySystemName(settings.Region);

            var credentials = string.IsNullOrEmpty(settings.AccessKeyID)
                ? null
                : new BasicAWSCredentials(settings.AccessKeyID, settings.SecretAccessKey);
            serviceCollection.AddDefaultAWSOptions(new AWSOptions
            {
                Region = region,
                Credentials = credentials,
            });
            serviceCollection.AddAWSService<IAmazonDynamoDB>();
        }
    }
}