using FluentValidation;

namespace WordoGuessr.API.BuildingBlocks.CQRS;

internal static class HandlerValidationRunner
{
    public static async Task ValidateAndThrow<TRequest>(
        IEnumerable<IValidator<TRequest>> validators,
        TRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var validatorArray = validators.ToArray();
        if (validatorArray.Length == 0)
        {
            return;
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            throw new ValidationException(failures);
        }
    }
}
