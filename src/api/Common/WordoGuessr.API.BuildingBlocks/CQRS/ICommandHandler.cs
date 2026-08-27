namespace WordoGuessr.API.BuildingBlocks.CQRS;

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken ct = default);
}
