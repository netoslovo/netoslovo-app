using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameStats;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameStats;

public static class GetDailyGameStatsEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGameStats(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/{day:DateOnly}/stats", Handle)
            .WithName("GetDailyGameStats");

        return group;
    }

    private static async Task<Results<
        Ok<GetDailyGameStatsResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromServices] IQueryHandler<GetDailyGameStatsQuery, Result<DailyGameStatsDto, GetDailyGameStatsError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetDailyGameStatsQuery(day);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(new GetDailyGameStatsResponse(result.Value));
    }

    private static Results<
        Ok<GetDailyGameStatsResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(GetDailyGameStatsError error) =>
        error switch
        {
            GetDailyGameStatsError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Daily game stats not found",
                    "Daily game stats not found",
                    error.ToString())),

            GetDailyGameStatsError.NotEnoughData =>
                TypedResults.Ok(new GetDailyGameStatsResponse(null)),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get daily game stats request failed",
                    "The Get daily game stats request could not be processed.",
                    error.ToString()))
        };
}
