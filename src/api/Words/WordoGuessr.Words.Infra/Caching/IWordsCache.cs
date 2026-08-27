namespace WordoGuessr.Words.Infra.Caching;

internal interface IWordsCache
{
    ValueTask<WordDistanceMapCacheEntry?> GetOrCreateWordSnapshot(
        ushort wordId,
        int version,
        Func<CancellationToken, Task<WordDistanceMapCacheEntry?>> factory,
        CancellationToken ct = default);

    ValueTask<WordsIndexCacheEntry?> GetOrCreateWordsIndex(
        int version,
        Func<CancellationToken, Task<WordsIndexCacheEntry?>> factory,
        CancellationToken ct = default);
}
