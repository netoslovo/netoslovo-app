using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetShare;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetShare;

public static class GetShareEndpoint
{
    public static IEndpointRouteBuilder MapGetShareSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/{gameId:guid}/share", Handle)
            .WithName("GetShare");

        return group;
    }

    private static async Task<Ok<GetShareResponse>> Handle(
        [FromRoute] Guid gameId,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetShareQuery, DailyGameShareDto?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetShareQuery(currentPlayer.PlayerId, gameId);

        var share = await handler.Handle(query, ct);

        return TypedResults.Ok(new GetShareResponse(share));
    }
}
