using Microsoft.AspNetCore.Diagnostics;

namespace WordoGuessr.WebAPI.Validation;

internal sealed class BadHttpRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
        {
            return false;
        }

        await TypedResults
            .ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["request"] = ["The request is invalid."]
                },
                title: "One or more validation errors occurred.")
            .ExecuteAsync(httpContext);

        return true;
    }
}
