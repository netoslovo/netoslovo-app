using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartForDay;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.StartForDay;

public static class StartForDayDailyEndpoint
{
    public static IEndpointRouteBuilder MapStartForDayDailySingleGame(this IEndpointRouteBuilder group)
    {
        group.MapPost("/daily/{day:DateOnly}", Handle)
            .ProducesValidationProblem()
            .WithName("StartForDayDailySingleGame");

        return group;
    }

    private static async Task<Results<
        Ok<GameDto>,
        Conflict<ProblemDetails>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] ICommandHandler<StartForDayDailyCommand, Result<GameDto, StartDailyError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var command = new StartForDayDailyCommand(day, currentPlayer.PlayerId);

        var result = await handler.Handle(command, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        return ToResult(result.Error, command);
    }

    private static Results<
        Ok<GameDto>,
        Conflict<ProblemDetails>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            StartDailyError error,
            StartForDayDailyCommand command) =>
        error switch
        {
            StartDailyError.AlreadyHasActiveGame =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Active game already exists",
                    "The current player already has an active game.",
                    error.ToString())),

            StartDailyError.ScheduleNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Daily schedule not found",
                    $"Daily schedule for day {command.Day} not found",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Game creation failed",
                    "The game creation request could not be processed.",
                    error.ToString()))
        };
}
