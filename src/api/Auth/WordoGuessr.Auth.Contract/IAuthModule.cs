namespace WordoGuessr.Auth.Contract;

public interface IAuthModule
{
    Task<IReadOnlyDictionary<Guid, string>> GetUserNames(
        IReadOnlyList<Guid> userIds, CancellationToken ct = default);
}
