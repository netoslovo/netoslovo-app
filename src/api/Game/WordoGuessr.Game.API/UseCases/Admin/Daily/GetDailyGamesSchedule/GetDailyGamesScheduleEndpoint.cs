using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetDailyGamesSchedule;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetDailyGamesSchedule;

internal static class GetDailyGamesScheduleEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGamesSchedule(this IEndpointRouteBuilder group)
    {
        group.MapGet("/schedule", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetDailyGamesSchedule");

        return group;
    }

    private static async Task<Results<
        Ok<DailyGameSchedulesDto>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromQuery] SortDirectionDto? sortDirection,
            [FromQuery] bool? assignedFilter,
            [FromServices] IQueryHandler<
                GetDailyGamesScheduleQuery,
                Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetDailyGamesScheduleQuery(
            from,
            to,
            skip,
            take,
            sortDirection,
            assignedFilter);

        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<DailyGameSchedulesDto>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            GetDailyGamesScheduleError error,
            GetDailyGamesScheduleQuery query) =>
        error switch
        {
            GetDailyGamesScheduleError.TooWideInterval =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Date interval is too wide",
                    $"The interval from '{query.From}' to '{query.To}' exceeds the supported maximum of five years.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get daily games schedule request failed",
                    "The daily games schedule request could not be processed.",
                    error.ToString()))
        };
}
