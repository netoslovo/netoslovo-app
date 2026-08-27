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
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetForDay;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetForDay;

public static class GetDailyGameForDayEndpoint
{
    public static IEndpointRouteBuilder MapGetDailyGameForDay(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/{day:DateOnly}", Handle)
            .WithName("GetDailyGameForDay");

        return group;
    }

    private static async Task<Results<
        Ok<GetDailyGameForDayResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] DateOnly day,
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] IQueryHandler<GetDailyGameForDayQuery, Result<GameDto?, GetDailyError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetDailyGameForDayQuery(day, currentPlayer.PlayerId);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(new GetDailyGameForDayResponse(result.Value));
    }

    private static Results<
        Ok<GetDailyGameForDayResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(GetDailyError error) =>
        error switch
        {
            GetDailyError.ScheduleNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Daily schedule not found",
                    "Daily schedule not found",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get daily game request failed",
                    "The Get daily game request could not be processed.",
                    error.ToString()))
        };
}
