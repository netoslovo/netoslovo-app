using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.Unshare;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.Unshare;

public static class UnshareEndpoint
{
    public static IEndpointRouteBuilder MapUnshareSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapDelete("/daily/{gameId:guid}/share", Handle)
            .ProducesValidationProblem()
            .WithName("Unshare");

        return group;
    }

    private static async Task<NoContent> Handle(
        [FromRoute] Guid gameId,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] ICommandHandler<UnshareCommand> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new UnshareCommand(currentPlayer.PlayerId, gameId);

        await handler.Handle(command, ct);

        return TypedResults.NoContent();
    }
}
