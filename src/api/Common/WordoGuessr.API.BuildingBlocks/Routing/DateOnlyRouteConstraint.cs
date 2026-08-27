using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace WordoGuessr.API.BuildingBlocks.Routing;

public sealed class DateOnlyRouteConstraint : IRouteConstraint
{
    private const string Format = "yyyy-MM-dd";

    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var value) || value is null)
        {
            return false;
        }

        return DateOnly.TryParseExact(
            Convert.ToString(value, CultureInfo.InvariantCulture),
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}
