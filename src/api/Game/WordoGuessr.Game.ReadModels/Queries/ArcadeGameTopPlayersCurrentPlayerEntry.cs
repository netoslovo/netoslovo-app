namespace WordoGuessr.Game.ReadModels.Queries;

public sealed class ArcadeGameTopPlayersCurrentPlayerEntry
{
    public Guid PlayerId { get; }
    public int? Place { get; }
    public int GuessedGames { get; }
    public decimal? AverageScore { get; }
    public TimeSpan? AverageDuration { get; }

    private ArcadeGameTopPlayersCurrentPlayerEntry()
    {
    }

    private ArcadeGameTopPlayersCurrentPlayerEntry(
        Guid playerId,
        int? place,
        int guessedGames,
        decimal? averageScore,
        TimeSpan? averageDuration)
    {
        PlayerId = playerId;
        Place = place;
        GuessedGames = guessedGames;
        AverageScore = averageScore;
        AverageDuration = averageDuration;
    }

    public static ArcadeGameTopPlayersCurrentPlayerEntry Ranked(
        Guid playerId,
        int place,
        int guessedGames,
        decimal averageScore,
        TimeSpan averageDuration) =>
        new ArcadeGameTopPlayersCurrentPlayerEntry(
            playerId,
            place,
            guessedGames,
            averageScore,
            averageDuration);

    public static ArcadeGameTopPlayersCurrentPlayerEntry NotRanked(Guid playerId) =>
        new ArcadeGameTopPlayersCurrentPlayerEntry(playerId, null, 0, null, null);
}
