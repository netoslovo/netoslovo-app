using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Common.MakeGuess;

public static class MakeGuessEndpoint
{
    public static IEndpointRouteBuilder MapMakeGuessSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{gameId:guid}/guesses", Handle)
            .ProducesValidationProblem()
            .WithName("SingleGameMakeGuess");

        return group;
    }

    private static async Task<Results<
        Ok<GuessOutcomeDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromBody] MakeGuessRequest request,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<MakeGuessCommand, Result<GuessOutcomeDto, MakeGuessError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new MakeGuessCommand(currentPlayer.PlayerId, gameId, request.Word);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<GuessOutcomeDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(MakeGuessError error, MakeGuessCommand command) =>
        error switch
        {
            MakeGuessError.WordNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Word was not found",
                    $"The word '{command.Word}' was not found in the dictionary.",
                    error.ToString())),

            MakeGuessError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Active game was not found",
                    $"Game '{command.GameId}' was not found for the current player or is no longer active.",
                    error.ToString())),

            MakeGuessError.InvalidInput =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Guess is invalid",
                    "The provided word does not satisfy the game input rules.",
                    error.ToString())),

            MakeGuessError.GameFinished =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game is already finished",
                    $"Game '{command.GameId}' is finished and no longer accepts guesses.",
                    error.ToString())),

            MakeGuessError.GameStateChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game state changed",
                    $"State of game '{command.GameId}' has been changed while processing the guess.",
                    error.ToString())),

            MakeGuessError.GameSourceNotFound =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game source was not found",
                    $"Source word of game '{command.GameId}' is no longer available, game has been cancelled.",
                    error.ToString())),

            MakeGuessError.WordsVersionChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Words version changed",
                    $"Dictionary version for game '{command.GameId}' has changed and current guesses were recalculated.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Guess request failed",
                    "The guess request could not be processed.",
                    error.ToString()))
        };
}
