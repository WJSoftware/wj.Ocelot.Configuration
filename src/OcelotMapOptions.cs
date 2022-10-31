using Ocelot.Configuration.File;

namespace wj.Ocelot.Configuration;

/// <summary>
/// Defines the call signature of the mapper function that is called once for every route inside the various route 
/// groups across all of the configuration.
/// </summary>
/// <typeparam name="TRouteGroup">The type of route group that will be used when declaring properties in the root 
/// gateway routes class.</typeparam>
/// <typeparam name="TRoute">The type of route that will be used in the route groups.</typeparam>
/// <param name="route">The route to transform into an Ocelot route.</param>
/// <param name="parent">The route's route group, which is used to access default values for properties the 
/// <paramref name="route" /> object does not provide.</param>
/// <param name="rootPath">The gateway's root path.</param>
/// <returns>A new <see cref="FileRoute" /> object, complete with all its meant configuration values.</returns>
public delegate FileRoute RouteMapperDelegate<TRouteGroup, TRoute>(TRoute route, TRouteGroup parent, string rootPath)
    where TRouteGroup : OcelotRouteGroup<TRoute>
    where TRoute : OcelotRoute;

/// <summary>
/// Options class that allows consumers of the library to configure the library's options.
/// </summary>
/// <typeparam name="TRoutes"></typeparam>
/// <typeparam name="TRouteGroup">The type of route group that will be used when declaring properties in the root 
/// gateway routes class.</typeparam>
/// <typeparam name="TRoute">The type of route that will be used in the route groups.</typeparam>
public sealed class OcelotMapOptions<TRoutes, TRouteGroup, TRoute>
    where TRoutes : GatewayRoutes<TRouteGroup, TRoute>
    where TRouteGroup : OcelotRouteGroup<TRoute>
    where TRoute : OcelotRoute
{
    #region Properties
    /// <summary>
    /// Gets or sets the delegate to call when mapping the configured routes to the configuration format that Ocelot 
    /// understands.
    /// </summary>
    /// <remarks>
    /// if not set, the library's default implementation will be used.  This default implementation will identify any 
    /// matches between the route or route group's properties and those properties of the <see cref="FileRoute" /> 
    /// class, and will transfer the values according to a simple rule:  The route's property value has the highest 
    /// priority, but if not set, then the route group's mathing property's value is used.  If niether the route or 
    /// the route group provide a value, then Ocelot's own default value will be in effect.
    /// <br />
    /// Specify yuor own mapping function here if your route type has properties that do not have a matching name or a 
    /// matching data type, therefore needing "manual" setting.
    /// <br />
    /// The property data type should be nullable, and nullable versions of non-nullable properties will still be 
    /// matched by the library's default implementation.
    /// </remarks>
    public RouteMapperDelegate<TRouteGroup, TRoute> MapperDelegate { get; set; }

    /// <summary>
    /// Gets the library's default mapper implementation.
    /// </summary>
    public RouteMapperDelegate<TRouteGroup, TRoute> DefaultMapperDelegate
        => OcelotRouteMapper<TRoutes, TRouteGroup, TRoute>.StdMapper;
    #endregion
}
