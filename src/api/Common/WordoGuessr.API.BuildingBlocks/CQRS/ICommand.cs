namespace WordoGuessr.API.BuildingBlocks.CQRS;

public interface ICommand { }

public interface ICommand<TResult> : ICommand { }
