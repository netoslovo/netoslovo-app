using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetSourceByWordWithReview;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetSourceByWordWithReview;

internal static class GetSourceByWordWithReviewEndpoint
{
    public static IEndpointRouteBuilder MapGetSourceByWordWithReview(this IEndpointRouteBuilder group)
    {
        group.MapGet("/sources/by-word", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetSourceByWordWithReview");

        return group;
    }

    private static async Task<Results<
        Ok<GameSourceWithReviewDto>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromQuery] string word,
            [FromServices] IQueryHandler<GetSourceByWordWithReviewQuery, Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetSourceByWordWithReviewQuery(word);
        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<GameSourceWithReviewDto>,
        NotFound<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            GetSourceByWordWithReviewError error,
            GetSourceByWordWithReviewQuery query) =>
        error switch
        {
            GetSourceByWordWithReviewError.InvalidWord =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Word is invalid",
                    $"Word '{query.Word}' is invalid.",
                    error.ToString())),

            GetSourceByWordWithReviewError.GameSourceNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Game source was not found",
                    $"Game source for word '{query.Word}' was not found.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get source by word request failed",
                    "The source lookup request could not be processed.",
                    error.ToString()))
        };
}
