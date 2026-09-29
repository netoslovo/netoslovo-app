using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.Share;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.Share;

public static class ShareEndpoint
{
    public static IEndpointRouteBuilder MapShareSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/daily/{gameId:guid}/share", Handle)
            .ProducesValidationProblem()
            .WithName("Share");

        return group;
    }

    private static async Task<Results<
        Ok<ShareResponse>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<ShareCommand, Result<Guid, ShareError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new ShareCommand(currentPlayer.PlayerId, gameId);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok(new ShareResponse(result.Value));
    }

    private static Results<
        Ok<ShareResponse>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            ShareError error,
            ShareCommand command) =>
        error switch
        {
            ShareError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Game was not found",
                    $"Game '{command.GameId}' was not found.",
                    error.ToString())),

            ShareError.ConcurrencyConflict =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game sharing conflict",
                    $"A concurrent request prevented sharing game '{command.GameId}'. Please try again.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Share request failed",
                    "The share request could not be processed.",
                    error.ToString()))
        };
}
