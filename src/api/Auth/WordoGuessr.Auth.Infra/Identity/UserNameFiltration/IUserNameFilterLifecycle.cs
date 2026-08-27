namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal interface IUserNameFilterLifecycle
{
    Task Init(int version, CancellationToken ct = default);
    Task<int> Update(int version, CancellationToken ct = default);
    int GetCurrentVersion();
}
