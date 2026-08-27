using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.AssignSourceToDay;

internal static class AssignSourceToDayEndpoint
{
    public static IEndpointRouteBuilder MapAssignSourceToDay(this IEndpointRouteBuilder group)
    {
        group.MapPut("/schedule/{day:DateOnly}/source/{approvedGameSourceId:long}", Handle)
            .RequireAuthorization(Permissions.AssignSource)
            .ProducesValidationProblem()
            .WithName("AssignSourceToDay");

        return group;
    }

    private static async Task<Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromRoute] long approvedGameSourceId,
            [FromServices] ICommandHandler<AssignSourceToDayCommand, Result<AssignSourceToDayError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var command = new AssignSourceToDayCommand(day, approvedGameSourceId);
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
        BadRequest<ProblemDetails>> ToResult(
            AssignSourceToDayError error,
            AssignSourceToDayCommand command) =>
        error switch
        {
            AssignSourceToDayError.ApprovedGameSourceNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Approved game source was not found",
                    $"Approved game source '{command.ApprovedGameSourceId}' was not found.",
                    error.ToString())),

            AssignSourceToDayError.DayFromPastLocked =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Daily schedule is locked",
                    $"The daily schedule for '{command.Day}' cannot be changed because the day is not in the future.",
                    error.ToString())),

            AssignSourceToDayError.ConcurrencyConflict =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Daily schedule was modified",
                    "The daily schedule was modified by another request. Retry the operation.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Assign source request failed",
                    "The source assignment request could not be processed.",
                    error.ToString()))
        };
}
