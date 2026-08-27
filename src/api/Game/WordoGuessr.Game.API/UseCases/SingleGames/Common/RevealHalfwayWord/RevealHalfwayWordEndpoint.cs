using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealHalfwayWord;

public static class RevealHalfwayWordEndpoint
{
    public static IEndpointRouteBuilder MapRevealHalfwayWord(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{gameId:guid}/hints/reveal-halfway-word", Handle)
            .ProducesValidationProblem()
            .WithName("RevealHalfwayWord");

        return group;
    }

    private static async Task<Results<
        Ok<GuessHintDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<RevealHalfwayWordHintCommand, Result<GuessHintDto, RevealHalfwayWordHintError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new RevealHalfwayWordHintCommand(currentPlayer.PlayerId, gameId);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<GuessHintDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            RevealHalfwayWordHintError error,
            RevealHalfwayWordHintCommand command) =>
        error switch
        {
            RevealHalfwayWordHintError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Active game was not found",
                    $"Game '{command.GameId}' was not found for the current player or is no longer active.",
                    error.ToString())),

            RevealHalfwayWordHintError.GameFinished =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game is already finished",
                    $"Game '{command.GameId}' is finished and no longer accepts guesses.",
                    error.ToString())),

            RevealHalfwayWordHintError.GameStateChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game state changed",
                    $"State of game '{command.GameId}' has been changed while processing the guess.",
                    error.ToString())),

            RevealHalfwayWordHintError.GameSourceNotFound =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game source was not found",
                    $"Source word of game '{command.GameId}' is no longer available, game has been cancelled.",
                    error.ToString())),

            RevealHalfwayWordHintError.WordsVersionChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Words version changed",
                    $"Dictionary version for game '{command.GameId}' has changed and current guesses were recalculated.",
                    error.ToString())),

            RevealHalfwayWordHintError.TooCloseToTarget =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Too close to target",
                    "You are too close to target to use this hint",
                    error.ToString())),

            RevealHalfwayWordHintError.UsageLimit =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Usage limit",
                    "You have been used all the possible hints of this type",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Apply closest neighbour hint request failed",
                    "The Apply closest neighbour hint request could not be processed.",
                    error.ToString()))
        };
}
