using System.Reflection;
using Ocelot.Configuration.File;

namespace wj.Ocelot.Configuration;

/// <summary>
/// Options class that represents the top of the hierarchy of the Ocelot configuration.
/// </summary>
/// <remarks>Inherit from this class and then add public read/write properties of type <typeparamref name="TRouteGroup" />, 
/// which is the <see cref="OcelotRouteGroup{TRoute}" /> type or a derived type, for every microservice that needs 
/// configuration.
/// <br />
/// <example>
/// Example:
/// <code>
///     public OcelotRouteGroup&lt;OcelotRoute&gt; Security { get; set; }
/// </code>
/// </example>
/// </remarks>
/// <typeparam name="TRouteGroup">The type of route group that will be used when declaring properties.</typeparam>
/// <typeparam name="TRoute">The type of route that will be used in the route groups.</typeparam>
public abstract class GatewayRoutes<TRouteGroup, TRoute>
    where TRoute : OcelotRoute
    where TRouteGroup : OcelotRouteGroup<TRoute>
{
    #region Properties
    /// <summary>
    /// Gets or sets the root path that is prepended to all upstream path templates.  In other words, sets the root 
    /// path assigned to the gateway.  This root path is prepended to all upstream paths.
    /// <br />
    /// <example>
    /// For example, a Kubernetes ingress could be set to route all requests that start with the path <c>/api</c> to 
    /// the gateway.
    /// <code>
    ///     RootPath = "/api";
    /// </code>
    /// </example>
    /// </summary>
    public string RootPath { get; set; }
    #endregion
}
