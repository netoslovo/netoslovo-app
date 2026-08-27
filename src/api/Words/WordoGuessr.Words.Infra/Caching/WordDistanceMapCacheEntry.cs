using MemoryPack;
using WordoGuessr.Words.Infra.WordsDistanceStorage;

namespace WordoGuessr.Words.Infra.Caching;

[MemoryPackable]
internal sealed partial record WordDistanceMapCacheEntry(
    ushort[] WordsIdsByDistance,
    ushort[] DistanceByWordsIds)
{
    public ushort GetWordIdByDistance(ushort distance)
    {
        if (distance >= WordsIdsByDistance.Length)
        {
            throw new WordDistancesStoreException(
                $"Distance index {distance} is outside the words by distance range of {WordsIdsByDistance.Length}.");
        }

        return WordsIdsByDistance[distance];
    }

    public ushort GetDistanceByWordId(ushort wordId)
    {
        if (wordId >= DistanceByWordsIds.Length)
        {
            throw new WordDistancesStoreException(
                $"Word id {wordId} is outside the distance by word id range of {DistanceByWordsIds.Length}.");
        }

        return DistanceByWordsIds[wordId];
    }

    public IReadOnlyList<ushort> GetClosestWordIds(ushort wordsCount)
    {
        if (wordsCount >= WordsIdsByDistance.Length)
        {
            throw new WordDistancesStoreException(
                $"Closest words count {wordsCount} is outside the words by distance range of {WordsIdsByDistance.Length}.");
        }

        return WordsIdsByDistance[1..(wordsCount + 1)];
    }
}
