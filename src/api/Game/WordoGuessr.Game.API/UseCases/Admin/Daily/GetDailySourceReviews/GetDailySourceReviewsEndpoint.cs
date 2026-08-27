using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetDailySourceReviews;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetDailySourceReviews;

internal static class GetDailySourceReviewsEndpoint
{
    public static IEndpointRouteBuilder MapGetDailySourceReviews(this IEndpointRouteBuilder group)
    {
        group.MapGet("/sources/reviews", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetDailySourceReviews");

        return group;
    }

    private static async Task<Results<
        Ok<DailyGameSourceReviewsDto>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromQuery] GameSourceReviewSortFieldDto? sortBy,
            [FromQuery] SortDirectionDto? sortDirection,
            [FromQuery] string? difficultyCodeFilter,
            [FromQuery] bool? existsInLatestVersionFilter,
            [FromQuery] bool? approvedFilter,
            [FromQuery] string? wordFilter,
            [FromQuery] bool? scheduledFilter,
            [FromServices] IQueryHandler<
                GetDailySourceReviewsQuery,
                Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetDailySourceReviewsQuery(
            skip,
            take,
            sortBy,
            sortDirection,
            difficultyCodeFilter,
            existsInLatestVersionFilter,
            approvedFilter,
            wordFilter,
            scheduledFilter);

        var result = await handler.Handle(query, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, query);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<
        Ok<DailyGameSourceReviewsDto>,
        UnprocessableEntity<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
        GetDailySourceReviewsError error,
        GetDailySourceReviewsQuery query) =>
        error switch
        {
            GetDailySourceReviewsError.InvalidDifficulty =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Difficulty is invalid",
                    $"Difficulty '{query.DifficultyCodeFilter}' is not supported.",
                    error.ToString())),

            GetDailySourceReviewsError.InvalidWord =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "Word is invalid",
                    $"Word '{query.WordFilter}' is invalid.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Get daily source reviews request failed",
                    "The daily source reviews request could not be processed.",
                    error.ToString()))
        };
}
