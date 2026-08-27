using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGamePlayerStats;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetDailyGamePlayerStats;

public static class GetDailyGamePlayerStatsEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGamePlayerStats(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/{day:DateOnly}/stats/player", Handle)
            .WithName("GetDailyGamePlayerStats");

        return group;
    }

    private static async Task<Results<
        Ok<DailyGamePlayerStatsDto>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] IQueryHandler<GetDailyGamePlayerStatsQuery, Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetDailyGamePlayerStatsQuery(currentPlayer.PlayerId, day);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<DailyGamePlayerStatsDto>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(GetDailyGamePlayerStatsError error) =>
        error switch
        {
            GetDailyGamePlayerStatsError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Daily game result not found",
                    "Daily game result not found",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get daily game player stats request failed",
                    "The Get daily game player stats request could not be processed.",
                    error.ToString()))
        };
}
