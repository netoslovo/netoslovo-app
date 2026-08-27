namespace WordoGuessr.Game.Domain;

public sealed class VersionedGameSource
{
    public long GameSourceId { get; }
    public GameSource GameSource { get; } = null!;
    public int WordsVersion { get; }
    public Difficulty Difficulty { get; } = null!;
    public int PredictedRawDifficultyValue { get; }

    public VersionedGameSource(
        long gameSourceId,
        int wordsVersion,
        Difficulty difficulty,
        int predictedRawDifficultyValue)
    {
        GameSourceId = gameSourceId;
        WordsVersion = wordsVersion;
        Difficulty = difficulty;
        PredictedRawDifficultyValue = predictedRawDifficultyValue;
    }

    private VersionedGameSource() { }
}
