namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal interface IUserNameReplacementList
{
    Task<IReadOnlyList<UserNameReplacement>> GetReplacements(int version, CancellationToken ct = default);
}
