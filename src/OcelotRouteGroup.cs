using Ocelot.Configuration.File;

namespace wj.Ocelot.Configuration;

/// <summary>
/// Options class that represents a group of Ocelot routes, capable of providing group-wide defaults to routes that do 
/// not set values for themselves.
/// </summary>
/// <typeparam name="TRoute">Type of route to use.</typeparam>
public class OcelotRouteGroup<TRoute>
    where TRoute : OcelotRoute
{
    #region Properties
    /// <summary>
    /// Gets or sets the host name for all the downstream paths contained in the <see cref="Routes" /> 
    /// property.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port number for all the downstream paths contained in the <see cref="Routes" /> 
    /// property.
    /// </summary>
    /// <remarks>If no port is specified, the default value is 80.</remarks>
    public int Port { get; set; } = 80;

    /// <summary>
    /// Gets or sets the HTTP scheme for all the downstream paths contained in the <see cref="Routes" /> 
    /// property.
    /// </summary>
    /// <remarks>If no scheme is specified, the default value is "http".</remarks>
    public string DownstreamScheme { get; set; } = "http";

    /// <summary>
    /// Gets or sets the time the gateway will wait for a downstream response before responding with a timeout 
    /// error code 504.
    /// </summary>
    /// <remarks>This property maps to Ocelot's <see cref="FileQoSOptions.TimeoutValue" /> property.</remarks>
    public TimeSpan? TimeOut { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the root path common to all upstream routes defined under this route group.
    /// </summary>
    /// <remarks>It is common practice to set a specific piece of path that uniquely identify a microservice or 
    /// similar construct.  That piece is specified here.
    /// <br />
    /// For example, if the route group defines routes for a RESTful microservice for the <c>User</c> model, it is 
    /// common to add something like <c>/user</c> as part of the resource identifier.  That piece is defined here.
    /// </remarks>
    public string RootPath { get; set; }

    /// <summary>
    /// Gets or sets the priority for all the routes contained within this route group.  If not set, it defaults to 
    /// zero, which is own's Ocelot default.
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Gets or sets an array of routes configured for this route group.
    /// </summary>
    public TRoute[] Routes { get; set; }

    #endregion
}
