namespace WordoGuessr.Auth.App.Services;

public interface IUserNameGenerator
{
    string Generate();
    string GenerateFromExtendedPool();
}
