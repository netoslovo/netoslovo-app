namespace WordoGuessr.Game.App.Abstractions;

public interface IGameStoreUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}