using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.ReadModels.Stored;

public sealed class ArcadeGameStats
{
    public Guid PlayerId { get; }
    public Difficulty Difficulty { get; } = null!;
    public int GuessedGames { get; private set; }
    public int TotalScore { get; private set; }
    public int TotalAttempts { get; private set; }
    public TimeSpan TotalDuration { get; private set; }
    public decimal AverageScore { get; private set; }
    public TimeSpan AverageDuration { get; private set; }

    private ArcadeGameStats() { }

    public ArcadeGameStats(Guid playerId, Difficulty difficulty)
    {
        PlayerId = playerId;
        Difficulty = difficulty;
    }

    public void RecordSuccessfulGame(int score, int attemptsCount, TimeSpan duration)
    {
        GuessedGames++;
        TotalScore += score;
        TotalAttempts += attemptsCount;
        TotalDuration += duration;
        AverageScore = Math.Round(TotalScore / (decimal)GuessedGames, 1, MidpointRounding.AwayFromZero);
        AverageDuration = TotalDuration / GuessedGames;
    }
}
