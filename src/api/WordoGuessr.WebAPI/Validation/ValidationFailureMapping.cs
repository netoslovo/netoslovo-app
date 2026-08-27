using FluentValidation.Results;

namespace WordoGuessr.WebAPI.Validation;

internal static class ValidationFailureMapping
{
    public static IDictionary<string, string[]> ToValidationErrors(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.Errors.ToValidationErrors();
    }

    public static IDictionary<string, string[]> ToValidationErrors(this IEnumerable<ValidationFailure> failures)
    {
        return failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .Distinct()
                    .ToArray());
    }
}
