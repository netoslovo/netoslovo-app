using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.UseCases.User.ChangeUserName;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.API.UseCases.User.ChangeUserName;

internal static class ChangeUserNameEndpoint
{
    public static IEndpointRouteBuilder MapChangeUserName(this IEndpointRouteBuilder group)
    {
        group.MapPut("/username", Handle)
            .WithName("ChangeUserName")
            .RequireAuthorization();

        return group;
    }

    private static async Task<Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ProblemHttpResult,
        BadRequest<ProblemDetails>>> Handle(
        [FromBody] ChangeUserNameRequest request,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] ICommandHandler<ChangeUserNameCommand, Result<ChangeUserNameError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var result = await handler.Handle(new ChangeUserNameCommand(currentPlayer.PlayerId, request.NewUserName), ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }
        return TypedResults.Ok();
    }

    private static Results<
        Ok,
        NotFound<ProblemDetails>,
        Conflict<ProblemDetails>,
        UnprocessableEntity<ProblemDetails>,
        ProblemHttpResult,
        BadRequest<ProblemDetails>> ToResult(ChangeUserNameError error) =>
        error switch
        {
            ChangeUserNameError.UserNotFound =>
                TypedResults.NotFound(ProblemDetailsMapping.Create(
                    StatusCodes.Status404NotFound,
                    "User was not found",
                    "The current user was not found.",
                    error.ToString())),

            ChangeUserNameError.InvalidUserName =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "User name is invalid",
                    "The specified user name is invalid.",
                    error.ToString())),

            ChangeUserNameError.UnsafeUserName =>
                TypedResults.UnprocessableEntity(ProblemDetailsMapping.Create(
                    StatusCodes.Status422UnprocessableEntity,
                    "User name is unsafe",
                    "The specified user name is not allowed.",
                    error.ToString())),

            ChangeUserNameError.DuplicateUserName =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "User name is already taken",
                    "The specified user name is already taken.",
                    error.ToString())),

            ChangeUserNameError.TooFrequentAttempts =>
                TypedResults.Problem(ProblemDetailsMapping.Create(
                    StatusCodes.Status429TooManyRequests,
                    "User name change limit exceeded",
                    "The user name was changed too recently. Please try again later.",
                    error.ToString())),

            ChangeUserNameError.ConcurrencyFailure =>
                TypedResults.Conflict(ProblemDetailsMapping.Create(
                    StatusCodes.Status409Conflict,
                    "User name change conflict",
                    "The user name change conflicted with another request. Please try again later.",
                    error.ToString())),

            ChangeUserNameError.Unauthorized =>
                TypedResults.Problem(ProblemDetailsMapping.Create(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "The current session is not authorized to change the user name.",
                    error.ToString())),

            _ =>
                TypedResults.BadRequest(ProblemDetailsMapping.Create(
                    StatusCodes.Status400BadRequest,
                    "User name change failed",
                    "The user name change request could not be processed.",
                    error.ToString()))
        };
}
