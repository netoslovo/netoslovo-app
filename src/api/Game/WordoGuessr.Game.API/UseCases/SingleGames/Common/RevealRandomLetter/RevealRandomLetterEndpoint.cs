using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealRandomLetter;

internal static class RevealRandomLetterEndpoint
{
    public static IEndpointRouteBuilder MapRevealRandomLetterSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{gameId:guid}/hints/random-letter", Handle)
            .ProducesValidationProblem()
            .WithName("RevealRandomLetter");

        return group;
    }

    private static async Task<Results<
        Ok<TextHintDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<RevealRandomLetterCommand, Result<TextHintDto, RevealRandomLetterError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new RevealRandomLetterCommand(currentPlayer.PlayerId, gameId);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<TextHintDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            RevealRandomLetterError error,
            RevealRandomLetterCommand command) =>
        error switch
        {
            RevealRandomLetterError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Active game was not found",
                    $"Game '{command.GameId}' was not found for the current player or is no longer active.",
                    error.ToString())),

            RevealRandomLetterError.GameFinished =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game is already finished",
                    $"Game '{command.GameId}' is finished and no longer accepts guesses.",
                    error.ToString())),

            RevealRandomLetterError.GameStateChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game state changed",
                    $"State of game '{command.GameId}' has been changed while processing the guess.",
                    error.ToString())),

            RevealRandomLetterError.LengthShouldBeRevealedFirst =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Length should be revealed first",
                    $"Word length of game {command.GameId} should be revealed first before revealing letters",
                    error.ToString())),

            RevealRandomLetterError.RevealLetterLimit =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Letter reveal limit reached",
                    $"All possible letter of the word for game {command.GameId} have been revealed",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Reveal random letter request failed",
                    "The Reveal random letter request could not be processed.",
                    error.ToString()))
        };
}
