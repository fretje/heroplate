using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Application;

public static class Startup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly().ConfigureMappers();
        return services;
    }

    public static void ConfigureMappers(this Assembly assembly)
    {
        foreach (var mapperConfigurationType in assembly.GetTypes()
            .Where(t => typeof(IMapperConfiguration).IsAssignableFrom(t) && t.IsClass))
        {
            if (Activator.CreateInstance(mapperConfigurationType) is not IMapperConfiguration mapperConfiguration)
            {
                throw new InvalidOperationException($"Couldn't create MapperConfiguration {mapperConfigurationType.Name}.");
            }

            mapperConfiguration.Configure();
        }
    }
}