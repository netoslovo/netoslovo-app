using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetLatestActive;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetLatestActive;

public static class GetLatestActiveArcadeSingleGameEndpoint
{
    public static IEndpointRouteBuilder MapGetLatestActiveArcadeSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapGet("/arcade/latest-active", Handle)
            .WithName("GetLatestActiveArcadeSingleGame");

        return group;
    }

    private static async Task<Ok<GetLatestActiveArcadeSingleGameResponse>> Handle(
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetLatestActiveArcadeQuery, ArcadeGameInfoDto?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var result = await handler.Handle(new GetLatestActiveArcadeQuery(currentPlayer.PlayerId), ct);
        return TypedResults.Ok(new GetLatestActiveArcadeSingleGameResponse(result));
    }
}
