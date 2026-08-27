using System.Diagnostics.CodeAnalysis;

namespace WordoGuessr.Game.Domain;

public sealed record Difficulty
{
    public string Code { get; } = null!;
    public string Name { get; } = null!;

    private Difficulty() { }

    private Difficulty(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly Difficulty Easy = new Difficulty("easy", "Легко");
    public static readonly Difficulty Medium = new Difficulty("medium", "Нормально");
    public static readonly Difficulty Hard = new Difficulty("hard", "Сложно");
    public static readonly Difficulty Impossible = new Difficulty("impossible", "Невозможно");

    public static IReadOnlyList<Difficulty> All => _all;

    public static Difficulty FromCode(string code)
    {
        return _difficultyMap.TryGetValue(code, out var difficulty)
            ? difficulty
            : throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown difficulty code");
    }

    public static bool TryFromCode(string code, [NotNullWhen(true)] out Difficulty? difficulty)
    {
        return _difficultyMap.TryGetValue(code, out difficulty);
    }

    private static readonly Difficulty[] _all = [Easy, Medium, Hard, Impossible];

    private static readonly Dictionary<string, Difficulty> _difficultyMap = _all.ToDictionary(x => x.Code);
}
