using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.UnassignSourceFromDay;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.UnassignSourceFromDay;

internal static class UnassignSourceFromDayEndpoint
{
    public static IEndpointRouteBuilder MapUnassignSourceFromDay(this IEndpointRouteBuilder group)
    {
        group.MapDelete("/schedule/{day:DateOnly}/source", Handle)
            .RequireAuthorization(Permissions.UnassignSource)
            .ProducesValidationProblem()
            .WithName("UnassignSourceFromDay");

        return group;
    }

    private static async Task<Results<
        Ok,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromServices] ICommandHandler<UnassignSourceFromDayCommand, Result<UnassignSourceFromDayError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var command = new UnassignSourceFromDayCommand(day);
        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok();
    }

    private static Results<
        Ok,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            UnassignSourceFromDayError error,
            UnassignSourceFromDayCommand command) =>
        error switch
        {
            UnassignSourceFromDayError.DayFromPastLocked =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Daily schedule is locked",
                    $"The daily schedule for '{command.Day}' cannot be changed because the day is not in the future.",
                    error.ToString())),

            UnassignSourceFromDayError.ConcurrencyConflict =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Daily schedule was modified",
                    "The daily schedule was modified by another request. Retry the operation.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Unassign source request failed",
                    "The source unassignment request could not be processed.",
                    error.ToString()))
        };
}
