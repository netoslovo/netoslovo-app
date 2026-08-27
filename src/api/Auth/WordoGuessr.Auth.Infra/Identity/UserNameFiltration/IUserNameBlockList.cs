namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal interface IUserNameBlockList
{
    Task<IReadOnlyList<string>> GetBlockedWords(int version, CancellationToken ct = default);
}