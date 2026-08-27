namespace WordoGuessr.Words.App.Abstractions;

public sealed class WordDistanceMap
{
    public int Version { get; }
    public int WordId { get; }
    public byte[] WordsIdsByDistance { get; } = [];
    public byte[] DistancesByWordsIds { get; } = [];

    public WordDistanceMap(
        int version,
        int wordId,
        byte[] wordsIdsByDistance,
        byte[] distancesByWordsIds)
    {
        Version = version;
        WordId = wordId;
        WordsIdsByDistance = wordsIdsByDistance;
        DistancesByWordsIds = distancesByWordsIds;
    }

    private WordDistanceMap() { }
}
