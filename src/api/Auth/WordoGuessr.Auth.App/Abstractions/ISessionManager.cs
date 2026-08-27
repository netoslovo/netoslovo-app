namespace WordoGuessr.Auth.App.Abstractions;

public interface ISessionManager
{
    Task SignIn(Guid userId);

    Task SignOut();

    Task RefreshSignIn(Guid userId);
}
