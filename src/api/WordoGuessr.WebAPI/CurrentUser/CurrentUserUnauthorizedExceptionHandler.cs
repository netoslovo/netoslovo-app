using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;

namespace WordoGuessr.WebAPI.CurrentUser;

internal sealed class CurrentUserUnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not CurrentPlayerException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Unauthorized",
            Type = "https://httpstatuses.com/401"
        };

        await TypedResults.Problem(problemDetails).ExecuteAsync(httpContext);
        return true;
    }
}
