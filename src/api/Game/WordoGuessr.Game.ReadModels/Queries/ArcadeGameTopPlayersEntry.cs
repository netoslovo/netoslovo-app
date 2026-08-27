namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record ArcadeGameTopPlayersEntry(
    Guid PlayerId,
    int Place,
    int GuessedGames,
    decimal AverageScore,
    TimeSpan AverageDuration);
