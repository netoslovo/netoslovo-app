using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetUnassignedDaysCount;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetUnassignedDaysCount;

internal static class GetUnassignedDaysCountEndpoint
{
    public static IEndpointRouteBuilder MapGetUnassignedDaysCount(this IEndpointRouteBuilder group)
    {
        group.MapGet("/schedule/unassigned/count", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetUnassignedDaysCount");

        return group;
    }

    private static async Task<Results<
        Ok<int>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromServices] IQueryHandler<
                GetUnassignedDaysCountQuery,
                Result<int, GetUnassignedDaysCountError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetUnassignedDaysCountQuery(from, to);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<int>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            GetUnassignedDaysCountError error,
            GetUnassignedDaysCountQuery query) =>
        error switch
        {
            GetUnassignedDaysCountError.TooWideInterval =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Date interval is too wide",
                    $"The interval from '{query.From}' to '{query.To}' exceeds the supported maximum of 365 days.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get unassigned days count request failed",
                    "The unassigned days count request could not be processed.",
                    error.ToString()))
        };
}
