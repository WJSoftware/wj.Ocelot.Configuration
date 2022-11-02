using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace wj.Ocelot.Configuration;

/// <summary>
/// Class that defines the necessary extension methods to configure Ocelot.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures Ocelot by reading the provided configuration section and mapping it to an instance of the specified 
    /// <typeparamref name="TRoutes" /> type, which is then used to build Ocelot's routes array.
    /// </summary>
    /// <typeparam name="TRoutes">Gateway routes type.</typeparam>
    /// <typeparam name="TRouteGroup">The type of route group that will be used when declaring properties in the root 
    /// gateway routes class.</typeparam>
    /// <typeparam name="TRoute">The type of route that will be used in the route groups.</typeparam>
    /// <param name="builder">Configuration builder.</param>
    /// <param name="configuration">Configuration section containing Ocelot's configuration values as defined by this 
    /// library.</param>
    /// <param name="configDelegate">Optional configuration delegate used to set this library's possible options.</param>
    /// <returns>The given configuration builder to allow for fluent syntax.</returns>
    public static IConfigurationBuilder AddOcelotConfiguration<TRoutes, TRouteGroup, TRoute>(
        this IConfigurationBuilder builder,
        TRoutes configuration,
        Action<OcelotMapOptions<TRoutes, TRouteGroup, TRoute>> configDelegate = null
    )
        where TRoutes : GatewayRoutes<TRouteGroup, TRoute>
        where TRouteGroup : OcelotRouteGroup<TRoute>
        where TRoute : OcelotRoute
    {
        OcelotMapOptions<TRoutes, TRouteGroup, TRoute> mapOptions = new();
        configDelegate?.Invoke(mapOptions);
        var mapperFn = mapOptions.MapperDelegate ?? mapOptions.DefaultMapperDelegate;
        dynamic routes = new
        {
            Routes = OcelotRouteMapper<TRoutes, TRouteGroup, TRoute>.BuildRoutes(configuration, mapperFn)
        };
        string routeText = JsonSerializer.Serialize(routes);
        // Stream will be disposed automatically after the configuration root is built.
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(routeText));
        builder.AddJsonStream(ms);
        return builder;
    }

    /// <summary>
    /// Configures Ocelot by reading the provided configuration section and mapping it to an instance of the specified 
    /// <typeparamref name="TRoutes" /> type, which is then used to build Ocelot's routes array.
    /// </summary>
    /// <typeparam name="TRoutes">Gateway routes type.</typeparam>
    /// <param name="builder">Configuration builder.</param>
    /// <param name="configuration">Configuration section containing Ocelot's configuration values as defined by this 
    /// library.</param>
    /// <returns>The given configuration builder to allow for fluent syntax.</returns>
    public static IConfigurationBuilder AddOcelotConfiguration<TRoutes>(
        this IConfigurationBuilder builder,
        TRoutes configuration
    )
        where TRoutes : GatewayRoutes<OcelotRouteGroup<OcelotRoute>, OcelotRoute>
        => builder.AddOcelotConfiguration<TRoutes, OcelotRouteGroup<OcelotRoute>, OcelotRoute>(configuration, opts => { });
}
