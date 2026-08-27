using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetHistory;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetHistory;

public static class GetDailyGamesHistoryEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGamesHistory(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily", Handle)
            .WithName("GetDailyGamesHistory");

        return group;
    }
    private static Task<DailyGamesHistoryDto> Handle(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetDailyGamesHistoryQuery, DailyGamesHistoryDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();

        var query = new GetDailyGamesHistoryQuery(currentPlayer.PlayerId, skip, take);

        return handler.Handle(query, ct);
    }
}