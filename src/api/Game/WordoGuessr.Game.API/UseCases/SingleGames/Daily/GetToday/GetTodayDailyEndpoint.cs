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
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetToday;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetToday;

public static class GetTodayDailyGameEndpoint
{
    public static IEndpointRouteBuilder MapGetTodayDailyGame(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/today", Handle)
            .WithName("GetTodayDailyGame");

        return group;
    }

    private static async Task<Results<
        Ok<GetTodayDailyGameResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
            [FromServices] IQueryHandler<GetTodayDailyGameQuery, Result<DailyGameInfoDto?, GetDailyError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetTodayDailyGameQuery(currentPlayer.PlayerId);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(new GetTodayDailyGameResponse(result.Value));
    }

    private static Results<
        Ok<GetTodayDailyGameResponse>,
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
