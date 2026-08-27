using FluentValidation;

namespace WordoGuessr.API.BuildingBlocks.CQRS;

public sealed class ValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _innerHandler;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationCommandHandlerDecorator(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
    {
        _innerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task Handle(TCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        await HandlerValidationRunner.ValidateAndThrow(_validators, command, ct);
        await _innerHandler.Handle(command, ct);
    }
}

public sealed class ValidationCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _innerHandler;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationCommandHandlerDecorator(
        ICommandHandler<TCommand, TResult> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
    {
        _innerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task<TResult> Handle(TCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        await HandlerValidationRunner.ValidateAndThrow(_validators, command, ct);
        return await _innerHandler.Handle(command, ct);
    }
}
