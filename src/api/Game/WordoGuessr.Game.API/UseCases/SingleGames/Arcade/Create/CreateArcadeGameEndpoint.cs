using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.Create;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Arcade.Create;

public static class CreateArcadeGameEndpoint
{
    public static IEndpointRouteBuilder MapCreateArcadeSingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/arcade", Handle)
            .ProducesValidationProblem()
            .WithName("CreateArcadeSingleGame");

        return group;
    }

    private static async Task<Results<
        Ok<GameDto>,
        Conflict<ProblemDetails>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromBody] CreateArcadeGameRequest request,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<CreateArcadeCommand, Result<GameDto, CreateArcadeError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new CreateArcadeCommand(currentPlayer.PlayerId, request.DifficultyCode);

        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<GameDto>,
        Conflict<ProblemDetails>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            CreateArcadeError error,
            CreateArcadeCommand command) =>
        error switch
        {
            CreateArcadeError.AlreadyHasTheSameGame =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "The same game already exists",
                    "The current player already has the same game.",
                    error.ToString())),

            CreateArcadeError.NoMorePossibleGames =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "No more games are available",
                    $"No more games are available for difficulty '{command.DifficultyCode}'.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Game creation failed",
                    "The game creation request could not be processed.",
                    error.ToString()))
        };
}
