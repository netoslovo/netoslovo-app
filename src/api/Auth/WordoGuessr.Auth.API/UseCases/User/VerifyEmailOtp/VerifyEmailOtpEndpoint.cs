using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.API.UseCases.User.VerifyEmailOtp;

internal static class VerifyEmailOtpEndpoint
{
    public static IEndpointRouteBuilder MapVerifyEmailOtp(this IEndpointRouteBuilder group)
    {
        group.MapPost("/email/verify-otp", Handle)
            .ProducesValidationProblem()
            .WithName("VerifyEmailOtp");

        return group;
    }

    private static async Task<Results<Ok, ProblemHttpResult, Conflict<ProblemDetails>>> Handle(
        VerifyEmailOtpRequest request,
        ICommandHandler<VerifyEmailOtpCommand, Result<VerifyEmailOtpError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await handler.Handle(new VerifyEmailOtpCommand(request.ChallengeId, request.Code), ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok();
    }

    private static Results<Ok, ProblemHttpResult, Conflict<ProblemDetails>> ToResult(VerifyEmailOtpError error) =>
        error switch
        {
            VerifyEmailOtpError.AlreadyAuthenticated => TypedResults.Conflict(ProblemDetailsMapping.Create(
                StatusCodes.Status409Conflict,
                "Already authenticated",
                "The current session is already authenticated.",
                error.ToString())),
            VerifyEmailOtpError.OtpNotFound => Unauthorized(
                "OTP challenge was not found",
                "The OTP challenge does not exist or has expired.",
                error),
            VerifyEmailOtpError.InvalidOtpState => Unauthorized(
                "OTP challenge is invalid",
                "The OTP challenge is no longer valid.",
                error),
            VerifyEmailOtpError.InvalidGuestSession => Unauthorized(
                "Guest session is invalid",
                "The guest session does not match the OTP challenge.",
                error),
            VerifyEmailOtpError.EmailAlreadyRegistered => TypedResults.Conflict(ProblemDetailsMapping.Create(
                StatusCodes.Status409Conflict,
                "Email was registered concurrently",
                "A user with this email was created by another request. Please try signing in again.",
                error.ToString())),
            VerifyEmailOtpError.ConcurrencyFailure => TypedResults.Conflict(ProblemDetailsMapping.Create(
                StatusCodes.Status409Conflict,
                "Sign in conflict",
                "The sign in request conflicted with another request. Please try again later.",
                error.ToString())),
            VerifyEmailOtpError.GetOrAddUserError => TypedResults.Problem(ProblemDetailsMapping.Create(
                StatusCodes.Status500InternalServerError,
                "Unable to sign in",
                "The user could not be created or updated.",
                error.ToString())),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };

    private static ProblemHttpResult Unauthorized(string title, string detail, VerifyEmailOtpError error) =>
        TypedResults.Problem(ProblemDetailsMapping.Create(
            StatusCodes.Status401Unauthorized,
            title,
            detail,
            error.ToString()));
}
