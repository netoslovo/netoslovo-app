using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.ReviewDailyGameSource;

internal static class ReviewDailyGameSourceEndpoint
{
    public static IEndpointRouteBuilder MapReviewDailyGameSource(this IEndpointRouteBuilder group)
    {
        group.MapPut("/sources/{gameSourceId:long}/review", Handle)
            .RequireAuthorization(Permissions.SetSourceReview)
            .ProducesValidationProblem()
            .WithName("ReviewDailyGameSource");

        return group;
    }

    private static async Task<Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>>> Handle(
            [FromRoute] long gameSourceId,
            [FromBody] ReviewDailyGameSourceRequest request,
            [FromServices] ICommandHandler<ReviewDailyGameSourceCommand, Result<ReviewDailyGameSourceError>> handler,
            CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(handler);

        var command = new ReviewDailyGameSourceCommand(gameSourceId, request.Approved);
        var result = await handler.Handle(command, ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error, command);
        }

        return TypedResults.Ok();
    }

    private static Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        BadRequest<ProblemDetails>> ToResult(
            ReviewDailyGameSourceError error,
            ReviewDailyGameSourceCommand command) =>
        error switch
        {
            ReviewDailyGameSourceError.GameSourceNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "Game source was not found",
                    $"Game source '{command.GameSourceId}' was not found.",
                    error.ToString())),

            ReviewDailyGameSourceError.AlreadyAssignedToGameLocked =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game source is locked",
                    $"Game source '{command.GameSourceId}' is locked since it has been already assigned to daily game.",
                    error.ToString())),

            ReviewDailyGameSourceError.ConcurrencyConflict =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "Game source review conflict",
                    "The game source review was changed concurrently. Retry the request.",
                    error.ToString())),

            _ => TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "Review game source request failed",
                    "The game source review request could not be processed.",
                    error.ToString()))
        };
}
