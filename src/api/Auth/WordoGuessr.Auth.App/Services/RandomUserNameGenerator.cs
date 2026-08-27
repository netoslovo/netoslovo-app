namespace WordoGuessr.Auth.App.Services;

public sealed class RandomUserNameGenerator : IUserNameGenerator
{
    private const string UserNameBase = "Игрок_";
    private const int UserNameStartNumberInc = 100_000;
    private const int UserNameEndNumberExc = 1_000_000;

    private const int UserNameExtendedStartNumberInc = 100_000;
    private const int UserNameExtendedEndNumberExc = 1_000_000_000;

    public string Generate()
    {
        var randomNumber = Random.Shared.Next(UserNameStartNumberInc, UserNameEndNumberExc);
        return $"{UserNameBase}{randomNumber}";
    }

    public string GenerateFromExtendedPool()
    {
        var randomNumber = Random.Shared.Next(UserNameExtendedStartNumberInc, UserNameExtendedEndNumberExc);
        return $"{UserNameBase}{randomNumber}";
    }
}
