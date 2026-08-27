using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetApprovedUnassignedDailySourcesCount;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetApprovedUnassignedDailySourcesCount;

internal static class GetApprovedUnassignedDailySourcesCountEndpoint
{
    public static IEndpointRouteBuilder MapGetApprovedUnassignedDailySourcesCount(this IEndpointRouteBuilder group)
    {
        group.MapGet("/sources/approved-unassigned/count", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetApprovedUnassignedDailySourcesCount");

        return group;
    }

    private static async Task<Ok<int>> Handle(
        [FromServices] IQueryHandler<GetApprovedUnassignedDailySourcesCountQuery, int> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var count = await handler.Handle(new GetApprovedUnassignedDailySourcesCountQuery(), ct);
        return TypedResults.Ok(count);
    }
}
