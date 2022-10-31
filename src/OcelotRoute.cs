using Ocelot.Configuration.File;

namespace wj.Ocelot.Configuration;

/// <summary>
/// Options class that represents a single Ocelot route.
/// </summary>
/// <remarks>
/// The properties defined in this (and any derived classes) should always be made nullable so it is straightforward 
/// to identify when the property was not set in configuration, so the owning route group (an object of type 
/// <see cref="OcelotRouteGroup{TRoute}" /> or a derived class) can have the opportunity to provide the value.
/// </remarks>
public class OcelotRoute
{
    #region Properties
    /// <summary>
    /// Gets or sets the downstream path template for this route.
    /// </summary>
    public string DownstreamPathTemplate { get; set; }

    /// <summary>
    /// Gets or sets the upstream path template for this route.
    /// </summary>
    /// <remarks>
    /// The full upstream path template is calculated by prepending the gateway's <see cref="GatewayRoutes{TRouteGroup, TRoute}.RootPath" /> 
    /// value, plus the route's owner's <see cref="OcelotRouteGroup{TRoute}.RootPath" /> value to the value of this 
    /// property.
    /// <br />
    /// <example>
    /// For example, if the gateway's root path is <c>/api</c> and the route group's root path is <c>/users</c>, and 
    /// this property's value is <c>/{id}</c>, then the final upstream path template will be:
    /// <br />
    /// <c>/api/users/{id}</c>
    /// </example>
    /// </remarks>
    public string UpstreamPathTemplate { get; set; }

    /// <summary>
    /// Gets or sets a list of HTTP methods accepted for the route.
    /// </summary>
    public List<string> UpstreamHttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the time the gateway will wait for a downstream response before responding with a timeout 
    /// error code 504.
    /// </summary>
    /// <remarks>This property maps to Ocelot's <see cref="FileQoSOptions.TimeoutValue" /> property.</remarks>
    public TimeSpan? TimeOut { get; set; }

    /// <summary>
    /// Gets or sets the route's priority.  If not set, it defaults to zero, which is own's Ocelot default.
    /// </summary>
    public int? Priority { get; set; }
    #endregion
}
