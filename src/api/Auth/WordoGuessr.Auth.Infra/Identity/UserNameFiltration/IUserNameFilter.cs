namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal interface IUserNameFilter
{
    Task<bool> IsSafe(string userName);
}