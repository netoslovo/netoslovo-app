using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;

public static class GetDailyGameCurrentPlayerStreakEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGameCurrentPlayerStreak(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/stats/player/streak", Handle)
            .WithName("GetDailyGameCurrentPlayerStreak");

        return group;
    }

    private static async Task<Ok<DailyGameStreakDto>> Handle(
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetDailyGameCurrentPlayerStreakQuery, DailyGameStreakDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetDailyGameCurrentPlayerStreakQuery(currentPlayer.PlayerId);
        var streak = await handler.Handle(query, ct);

        return TypedResults.Ok(streak);
    }
}
