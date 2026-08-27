using WordoGuessr.Words.Infra.Caching;

namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

internal interface IDistancesPersistenceCacheAdapter
{
    Task<int> GetVersion(CancellationToken ct = default);

    Task<WordsIndexCacheEntry?> GetWordIndex(int version, CancellationToken ct = default);

    Task<WordDistanceMapCacheEntry?> GetWordSnapshot(
        int sourceWordId,
        int version,
        CancellationToken ct = default);
}
