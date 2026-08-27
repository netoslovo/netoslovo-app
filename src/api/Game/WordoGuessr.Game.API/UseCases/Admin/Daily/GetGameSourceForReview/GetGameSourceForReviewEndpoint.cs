using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetGameSourceForReview;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetGameSourceForReview;

internal static class GetGameSourceForReviewEndpoint
{
    public static IEndpointRouteBuilder MapGetGameSourceForReview(this IEndpointRouteBuilder group)
    {
        group.MapGet("/sources/{gameSourceId:long}/for-review", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetGameSourceForReview");

        return group;
    }

    private static async Task<Results<
        Ok<UnreviewedSourceDto>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] long gameSourceId,
            [FromQuery] int closestWordsCount,
            [FromServices] IQueryHandler<GetGameSourceForReviewQuery, Result<UnreviewedSourceDto, GetGameSourceForReviewError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetGameSourceForReviewQuery(gameSourceId, closestWordsCount);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<UnreviewedSourceDto>,
        NotFound<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            GetGameSourceForReviewError error,
            GetGameSourceForReviewQuery query) =>
        error switch
        {
            GetGameSourceForReviewError.GameSourceNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Game source was not found",
                    $"Game source '{query.GameSourceId}' was not found.",
                    error.ToString())),

            _ => TypedResults.BadRequest(ProblemDetailsMapping.Create(
                StatusCodes.Status400BadRequest,
                "Get game source for review request failed",
                "The game source could not be prepared for review.",
                error.ToString()))
        };
}
