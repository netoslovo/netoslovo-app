using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.API.Security;
using WordoGuessr.Auth.App.UseCases.Admin.GetCurrentUserNameFilterVersion;

namespace WordoGuessr.Auth.API.UseCases.Admin;

internal static class GetCurrentUserNameFilterVersionEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentUserNameFilterVersion(this IEndpointRouteBuilder group)
    {
        group.MapGet("/username-filter/version/", Handle)
            .WithName("GetCurrentUserNameFilterVersion")
            .RequireAuthorization(Permissions.ManageUserNameFilter);

        return group;
    }

    private static async Task<Ok<int>> Handle(
        [FromServices] IQueryHandler<GetCurrentUserNameFilterVersionQuery, int> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var version = await handler.Handle(new GetCurrentUserNameFilterVersionQuery(), ct);
        return TypedResults.Ok(version);
    }
}
