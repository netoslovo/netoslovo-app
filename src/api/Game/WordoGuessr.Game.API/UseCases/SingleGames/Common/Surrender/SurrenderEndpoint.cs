using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.Surrender;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Common.Surrender;

public static class SurrenderEndpoint
{
    public static IEndpointRouteBuilder MapSurrenderSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{gameId:guid}/surrender", Handle)
            .ProducesValidationProblem()
            .WithName("SurrenderSingleGame");

        return group;
    }

    private static async Task<Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<SurrenderCommand, Result<SurrenderError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new SurrenderCommand(currentPlayer.PlayerId, gameId);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok();
    }

    private static Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(SurrenderError error, SurrenderCommand command) =>
        error switch
        {
            SurrenderError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Active game was not found",
                    $"Game '{command.GameId}' was not found for the current player or is no longer active.",
                    error.ToString())),

            SurrenderError.GameFinished =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game is already finished",
                    $"Game '{command.GameId}' is finished and no longer accepts guesses.",
                    error.ToString())),

            SurrenderError.GameStateChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game state changed",
                    $"State of game '{command.GameId}' has been changed while processing the guess.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Surrender request failed",
                    "The surrender request could not be processed.",
                    error.ToString()))
        };
}
