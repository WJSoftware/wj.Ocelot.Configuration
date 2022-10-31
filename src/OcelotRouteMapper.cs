using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ocelot.Configuration.File;

namespace wj.Ocelot.Configuration;

internal static class OcelotRouteMapper<TRoutes, TRouteGroup, TRoute>
    where TRoutes : GatewayRoutes<TRouteGroup, TRoute>
    where TRouteGroup : OcelotRouteGroup<TRoute>
    where TRoute : OcelotRoute
{
    private class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
    {
        private static Type GetUnderlyingType(Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        public bool Equals(PropertyInfo x, PropertyInfo y)
            => x.Name == y.Name && GetUnderlyingType(x.PropertyType) == GetUnderlyingType(y.PropertyType);

        public int GetHashCode([DisallowNull] PropertyInfo obj)
            => obj.GetHashCode();
    }

    /// <summary>
    /// Lists all properties of this class that have a property type of <see cref="OcelotRouteGroup{TRoute}" /> or 
    /// a derived class.
    /// </summary>
    /// <returns>An enumerable collection of property information objects that satisfied the criteria.</returns>
    private static IEnumerable<PropertyInfo> GetAllRouteGroupProperties()
        => typeof(TRoutes).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.PropertyType == typeof(TRouteGroup));

    private static IList<PropertyInfo> GetAllProperties(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    private static Lazy<IList<(PropertyInfo FrPi, PropertyInfo OrPi, PropertyInfo OrgPi)>> _routeProperties;

    private static readonly string[] specialRouteProperties = new[]
    {
        nameof(OcelotRoute.TimeOut),
        nameof(OcelotRoute.UpstreamPathTemplate),
        nameof(OcelotRouteGroup<OcelotRoute>.Routes),
        nameof(OcelotRouteGroup<OcelotRoute>.Host),
        nameof(OcelotRouteGroup<OcelotRoute>.Port)
    };


    static OcelotRouteMapper()
    {
        _routeProperties = new Lazy<IList<(PropertyInfo FrPi, PropertyInfo OrPi, PropertyInfo OrgPi)>>(() =>
        {
            PropertyInfoComparer piComparer = new();
            List<(PropertyInfo FrPi, PropertyInfo OrPi, PropertyInfo OrgPi)> routeProperties = new();
            var frProperties = GetAllProperties(typeof(FileRoute));
            var orProperties = GetAllProperties(typeof(TRoute));
            var orgProperties = GetAllProperties(typeof(TRouteGroup));
            foreach (var pi in orProperties)
            {
                if (specialRouteProperties.Contains(pi.Name))
                {
                    // Ignore special properties.
                    continue;
                }
                var frPi = frProperties.Where(x => piComparer.Equals(x, pi)).FirstOrDefault();
                if (frPi != null)
                {
                    routeProperties.Add((frPi, pi, null));
                }
            }
            foreach (var pi in orgProperties)
            {
                if (specialRouteProperties.Contains(pi.Name))
                {
                    // Ignore special properties.
                    continue;
                }
                var index = routeProperties.FindIndex(0, x => piComparer.Equals(x.FrPi, pi));
                if (index >= 0)
                {
                    var curValue = routeProperties[index];
                    curValue.OrgPi = pi;
                    routeProperties[index] = curValue;
                }
                else
                {
                    var frPi = frProperties.Where(x => piComparer.Equals(x, pi)).FirstOrDefault();
                    if (frPi != null)
                    {
                        routeProperties.Add((frPi, null, pi));
                    }
                }
            }
            return routeProperties;
        });
    }

    internal static FileRoute StdMapper(TRoute route, TRouteGroup parent, string rootPath)
    {
        FileRoute fr = new();
        // Set the properties that match 1:1.
        foreach (var piGroup in _routeProperties.Value)
        {
            var orValue = piGroup.OrPi?.GetValue(route);
            var orgValue = piGroup.OrgPi?.GetValue(parent);
            var finalValue = orValue ?? orgValue;
            if (finalValue != null)
            {
                piGroup.FrPi.SetValue(fr, finalValue);
            }
        }
        // Set the properties that require special handling.
        fr.UpstreamPathTemplate = $"{rootPath}{parent.RootPath}{route.UpstreamPathTemplate ?? route.DownstreamPathTemplate}";
        fr.QoSOptions.TimeoutValue = (int)(route.TimeOut ?? parent.TimeOut ?? TimeSpan.Zero).TotalMilliseconds;
        return fr;
    }

    internal static IList<FileRoute> BuildRoutes(TRoutes gatewayRoutes, RouteMapperDelegate<TRouteGroup, TRoute> mapperFn)
    {
        List<FileRoute> routes = new List<FileRoute>();
        foreach (PropertyInfo pi in GetAllRouteGroupProperties())
        {
            TRouteGroup routeGroup = (TRouteGroup)pi.GetValue(gatewayRoutes);
            if ((routeGroup?.Routes?.Length ?? 0) == 0)
            {
                continue;
            }
            FileHostAndPort hap = new FileHostAndPort()
            {
                Host = routeGroup.Host,
                Port = routeGroup.Port
            };
            foreach (TRoute route in routeGroup.Routes)
            {
                var fr = mapperFn(route, routeGroup, gatewayRoutes.RootPath);
                fr.DownstreamHostAndPorts.Add(hap);
                routes.Add(fr);
            }
        }
        return routes;
    }
}
