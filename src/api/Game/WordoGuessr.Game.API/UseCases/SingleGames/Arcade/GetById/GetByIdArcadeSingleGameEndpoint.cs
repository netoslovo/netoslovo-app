using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetById;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetById;

public static class GetByIdArcadeSingleGameEndpoint
{
    public static IEndpointRouteBuilder MapGetByIdArcadeSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapGet("/arcade/{id:guid}", Handle)
            .WithName("GetByIdArcadeSingleGame");

        return group;
    }

    private static async Task<Ok<GetByIdArcadeSingleGameResponse>> Handle(
        [FromRoute] Guid id,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetByIdArcadeQuery, GameDto?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var result = await handler.Handle(new GetByIdArcadeQuery(currentPlayer.PlayerId, id), ct);
        return TypedResults.Ok(new GetByIdArcadeSingleGameResponse(result));
    }
}
