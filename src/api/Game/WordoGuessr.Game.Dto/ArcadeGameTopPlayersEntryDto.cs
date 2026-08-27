namespace WordoGuessr.Game.Dto;

public sealed record ArcadeGameTopPlayersEntryDto(
    int Place,
    string PlayerName,
    int GuessedGames,
    decimal AverageScore,
    TimeSpan AverageDuration);
