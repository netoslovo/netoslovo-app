using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetSharedGame;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetSharedGame;

public static class GetSharedGameEndpoint
{
    public static IEndpointRouteBuilder MapGetSharedGame(this IEndpointRouteBuilder group)
    {
        group.MapGet("/daily/shared/{publicId:guid}", Handle)
            .ProducesValidationProblem()
            .WithName("GetSharedGame");

        return group;
    }

    private static async Task<Results<
        Ok<GetSharedGameResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
        [FromRoute] Guid publicId,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetSharedGameQuery, Result<SharedDailyGameDto, GetSharedGameError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetSharedGameQuery(publicId, currentPlayer.PlayerId);
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(new GetSharedGameResponse(result.Value));
    }

    private static Results<
        Ok<GetSharedGameResponse>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
        GetSharedGameError error,
        GetSharedGameQuery query) =>
        error switch
        {
            GetSharedGameError.GameNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Shared game was not found",
                    $"Shared game '{query.PublicId}' was not found.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get shared game request failed",
                    "The shared game request could not be processed.",
                    error.ToString()))
        };
}
