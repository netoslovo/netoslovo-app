using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentStreakTop;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameCurrentStreakTop;

public static class GetDailyGameCurrentStreakTopEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGameCurrentStreakTop(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/stats/streak/current/top", Handle)
            .WithName("GetDailyGameCurrentStreakTop");

        return group;
    }

    private static async Task<Ok<DailyGameStreakTopDto>> Handle(
        [FromQuery] int topN,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetDailyGameCurrentStreakTopQuery, DailyGameStreakTopDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetDailyGameCurrentStreakTopQuery(currentPlayer.PlayerId, topN);
        var top = await handler.Handle(query, ct);

        return TypedResults.Ok(top);
    }
}
