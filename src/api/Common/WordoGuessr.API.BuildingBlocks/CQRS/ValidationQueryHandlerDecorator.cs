using FluentValidation;

namespace WordoGuessr.API.BuildingBlocks.CQRS;

public sealed class ValidationQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _innerHandler;
    private readonly IEnumerable<IValidator<TQuery>> _validators;

    public ValidationQueryHandlerDecorator(
        IQueryHandler<TQuery, TResult> innerHandler,
        IEnumerable<IValidator<TQuery>> validators)
    {
        _innerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task<TResult> Handle(TQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        await HandlerValidationRunner.ValidateAndThrow(_validators, query, ct);
        return await _innerHandler.Handle(query, ct);
    }
}
