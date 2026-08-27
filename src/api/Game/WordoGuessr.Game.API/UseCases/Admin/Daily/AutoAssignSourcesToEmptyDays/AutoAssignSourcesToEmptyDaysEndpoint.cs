using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.AutoAssignSourcesToEmptyDays;

internal static class AutoAssignSourcesToEmptyDaysEndpoint
{
    public static IEndpointRouteBuilder MapAutoAssignSourcesToEmptyDays(this IEndpointRouteBuilder group)
    {
        group.MapPost("/sources/auto-assign", Handle)
            .RequireAuthorization(Permissions.AssignSource)
            .ProducesValidationProblem()
            .WithName("AutoAssignSourcesToEmptyDays");

        return group;
    }

    private static async Task<Results<
        Ok,
        UnprocessableEntity<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromServices] ICommandHandler<AutoAssignSourcesToEmptyDaysCommand, Result<AutoAssignSourcesToEmptyDaysError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var command = new AutoAssignSourcesToEmptyDaysCommand(from, to);
        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok();
    }

    private static Results<
        Ok,
        UnprocessableEntity<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            AutoAssignSourcesToEmptyDaysError error,
            AutoAssignSourcesToEmptyDaysCommand command) =>
        error switch
        {
            AutoAssignSourcesToEmptyDaysError.NotEnoughApprovedSources =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Not enough approved sources",
                    $"There are not enough approved sources to assign every empty day from '{command.From}' to '{command.To}'.",
                    error.ToString())),

            AutoAssignSourcesToEmptyDaysError.TooWideInterval =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Date interval is too wide",
                    $"The interval from '{command.From}' to '{command.To}' exceeds the supported maximum.",
                    error.ToString())),

            AutoAssignSourcesToEmptyDaysError.ConcurrencyConflict =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Daily schedule was modified",
                    "The daily schedule was modified by another request. Retry the operation.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Auto-assign sources request failed",
                    "The auto-assign sources request could not be processed.",
                    error.ToString()))
        };
}
