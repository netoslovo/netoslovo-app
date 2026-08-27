namespace WordoGuessr.Auth.App.Abstractions;

public interface IAuthTransactionScope : IDisposable
{
    Task Commit(CancellationToken ct = default);
}
