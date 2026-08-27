using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;

namespace WordoGuessr.WebAPI.Validation;

internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors.ToValidationErrors();

        await TypedResults.ValidationProblem(
                errors,
                title: "One or more validation errors occurred.")
            .ExecuteAsync(httpContext);

        return true;
    }
}
