using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Infra.Caching;
using WordoGuessr.Words.Infra.Database;
using WordoGuessr.Words.Infra.Helpers;

namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

internal sealed class EfDbPersistenceCacheAdapter : IDistancesPersistenceCacheAdapter
{
    private const int UshortSize = sizeof(ushort);
    private readonly WordsDbContext _wordsDbContext;

    public EfDbPersistenceCacheAdapter(WordsDbContext wordsDbContext)
    {
        _wordsDbContext = wordsDbContext;
    }

    public async Task<int> GetVersion(CancellationToken ct = default)
    {
        var version = await _wordsDbContext.WordsVersions
            .AsNoTracking()
            .SingleAsync(wv => wv.State == WordsVersionState.Active, ct);

        return version.Version;
    }

    public async Task<WordsIndexCacheEntry?> GetWordIndex(int version, CancellationToken ct = default)
    {
        var wordsIndexes = await _wordsDbContext.WordsIndexes
            .AsNoTracking()
            .Where(wi => wi.Version == version)
            .OrderBy(wi => wi.WordId)
            .ToArrayAsync(ct);

        if (wordsIndexes.Length == 0) return null;

        var wordsByIds = wordsIndexes
            .Select(wi => wi.WordText)
            .ToArray();

        var wordsIds = wordsIndexes
            .ToDictionary(wi => wi.WordText, wi => Converters.ToUshort(wi.WordId));

        var wordsIndex = new WordsIndexCacheEntry(version, wordsByIds, wordsIds);
        return wordsIndex;
    }

    public async Task<WordDistanceMapCacheEntry?> GetWordSnapshot(int sourceWordId, int version, CancellationToken ct = default)
    {
        var snapshotEntity = await _wordsDbContext.WordsSnapshots
            .AsNoTracking()
            .SingleOrDefaultAsync(
                ws => ws.Version == version && ws.WordId == sourceWordId,
                ct);

        if (snapshotEntity is null) return null;

        var wordsIdsByDistance = ReadUshortArray(snapshotEntity.WordsIdsByDistance);
        var distancesByWordsIds = ReadUshortArray(snapshotEntity.DistancesByWordsIds);

        var snapshot = new WordDistanceMapCacheEntry(wordsIdsByDistance, distancesByWordsIds);
        return snapshot;
    }

    private static ushort[] ReadUshortArray(byte[] buffer)
    {
        var result = new ushort[buffer.Length / UshortSize];
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = ReadUshort(buffer, index);
        }

        return result;
    }

    private static ushort ReadUshort(byte[] buffer, int index) =>
        BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(index * UshortSize, UshortSize));
}
