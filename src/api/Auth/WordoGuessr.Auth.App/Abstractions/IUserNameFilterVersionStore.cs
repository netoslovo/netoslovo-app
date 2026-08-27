namespace WordoGuessr.Auth.App.Abstractions;

public interface IUserNameFilterVersionStore
{
    Task PublishNewVersion(int version, CancellationToken ct = default);
    Task<int> GetLatestVersion(CancellationToken ct = default);
}