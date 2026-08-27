namespace WordoGuessr.Game.Dto;

public sealed record ArcadeGameTopPlayersCurrentPlayerEntryDto(
    Guid PlayerId,
    int? Place,
    string PlayerName,
    int GuessedGames,
    decimal? AverageScore,
    TimeSpan? AverageDuration);
