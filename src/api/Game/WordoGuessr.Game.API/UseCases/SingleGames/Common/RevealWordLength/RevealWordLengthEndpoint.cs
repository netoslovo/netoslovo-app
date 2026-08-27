using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealWordLength;

public static class RevealWordLengthEndpoint
{
    public static IEndpointRouteBuilder MapRevealWordLengthSinglGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{gameId:guid}/hints/word-length", Handle)
            .ProducesValidationProblem()
            .WithName("RevealWordLength");

        return group;
    }

    private static async Task<Results<
        Ok<TextHintDto>,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] Guid gameId,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<RevealWordLengthCommand, Result<TextHintDto, RevealWordLengthError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new RevealWordLengthCommand(currentPlayer.PlayerId, gameId);

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
            RevealWordLengthError error,
            RevealWordLengthCommand command) =>
        error switch
        {
            RevealWordLengthError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Active game was not found",
                    $"Game '{command.GameId}' was not found for the current player or is no longer active.",
                    error.ToString())),

            RevealWordLengthError.GameFinished =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game is already finished",
                    $"Game '{command.GameId}' is finished and no longer accepts guesses.",
                    error.ToString())),

            RevealWordLengthError.GameStateChanged =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game state changed",
                    $"State of game '{command.GameId}' has been changed while processing the guess.",
                    error.ToString())),

            RevealWordLengthError.LengthAlreadyRevealed =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Length already revealed",
                    $"Word length of game {command.GameId} has been already revealed",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Reveal word length request failed",
                    "The Reveal word length request could not be processed.",
                    error.ToString()))
        };
}
