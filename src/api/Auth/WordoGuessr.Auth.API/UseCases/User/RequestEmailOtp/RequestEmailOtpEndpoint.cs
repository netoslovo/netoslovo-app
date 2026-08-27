using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.ProblemDetails;
using WordoGuessr.Auth.App.UseCases.User.RequestEmailOtp;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.API.UseCases.User.RequestEmailOtp;

internal static class RequestEmailOtpEndpoint
{
    public static IEndpointRouteBuilder MapRequestEmailOtp(this IEndpointRouteBuilder group)
    {
        group.MapPost("/email/request-otp", Handle)
            .ProducesValidationProblem()
            .WithName("RequestEmailOtp");

        return group;
    }

    private static async Task<Results<Ok<Guid>, Conflict<ProblemDetails>, ProblemHttpResult>> Handle(
        RequestEmailOtpRequest request,
        ICommandHandler<RequestEmailOtpCommand, Result<Guid, RequestEmailOtpError>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await handler.Handle(new RequestEmailOtpCommand(request.Email), ct);
        if (!result.IsSuccess)
        {
            return ToResult(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static Results<Ok<Guid>, Conflict<ProblemDetails>, ProblemHttpResult> ToResult(RequestEmailOtpError error) =>
        error switch
        {
            RequestEmailOtpError.AlreadyAuthenticated => TypedResults.Conflict(ProblemDetailsMapping.Create(
                StatusCodes.Status409Conflict,
                "Already authenticated",
                "The current session is already authenticated.",
                error.ToString())),
            RequestEmailOtpError.RateLimitError => TypedResults.Problem(ProblemDetailsMapping.Create(
                StatusCodes.Status429TooManyRequests,
                "OTP request limit exceeded",
                "Too many OTP codes have been requested. Please try again later.",
                error.ToString())),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
}
