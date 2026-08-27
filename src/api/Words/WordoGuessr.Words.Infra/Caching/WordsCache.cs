using WordoGuessr.Words.App.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace WordoGuessr.Words.Infra.Caching;

internal sealed class WordsCache : IWordsCache, IWordsCacheInvalidator
{
    public const string Name = "words:infra:";
    private const string WordKeyPrefix = "word-snapshot";
    private const string WordsIndexPrefix = "words-index";
    private const string VersionTagPrefix = "words-version";

    private readonly IFusionCache _fusionCache;

    public WordsCache(IFusionCacheProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _fusionCache = provider.GetCache(Name);
    }

    public ValueTask<WordDistanceMapCacheEntry?> GetOrCreateWordSnapshot(
        ushort wordId,
        int version,
        Func<CancellationToken, Task<WordDistanceMapCacheEntry?>> factory,
        CancellationToken ct = default)
    {
        return _fusionCache.GetOrSetAsync(
            BuildWordKey(wordId, version),
            factoryCt => factory(factoryCt),
            options: null,
            tags: [BuildVersionTag(version)],
            token: ct);
    }

    public ValueTask<WordsIndexCacheEntry?> GetOrCreateWordsIndex(
        int version,
        Func<CancellationToken, Task<WordsIndexCacheEntry?>> factory,
        CancellationToken ct = default)
    {
        return _fusionCache.GetOrSetAsync(
            BuildWordsIndexKey(version),
            factoryCt => factory(factoryCt),
            options: null,
            tags: [BuildVersionTag(version)],
            token: ct);
    }

    public ValueTask RemoveVersion(int version, CancellationToken ct = default)
    {
        return _fusionCache.RemoveByTagAsync(BuildVersionTag(version), token: ct);
    }

    public async ValueTask RemoveVersions(int upToVersion, CancellationToken ct = default)
    {
        for (int version = 0; version < upToVersion; version++)
        {
            await RemoveVersion(version, ct);
        }
    }

    public ValueTask Clear(CancellationToken ct = default)
    {
        return _fusionCache.ClearAsync(allowFailSafe: false, token: ct);
    }

    private static string BuildWordKey(ushort wordId, int version) => $"{WordKeyPrefix}:{wordId}:{version}";
    private static string BuildWordsIndexKey(int version) => $"{WordsIndexPrefix}:{version}";
    private static string BuildVersionTag(int version) => $"{VersionTagPrefix}:{version}";
}
