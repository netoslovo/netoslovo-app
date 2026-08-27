using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetHistory;

public static class GetArcadeGamesHistoryEndpoint
{
    public static IEndpointRouteBuilder MapGetArcadeGamesHistory(this IEndpointRouteBuilder group)
    {
        group.MapGet("/arcade", Handle)
            .WithName("GetArcadeGamesHistory");

        return group;
    }

    private static Task<ArcadeGamesHistoryDto> Handle(
        [FromQuery] int skip,
        [FromQuery] int take,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetArcadeGamesHistoryQuery, ArcadeGamesHistoryDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();

        var query = new GetArcadeGamesHistoryQuery(currentPlayer.PlayerId, skip, take);

        return handler.Handle(query, ct);
    }
}