using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract;
using WordoGuessr.Words.Infra.Caching;

namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

internal sealed class CachedWordsDistanceStore :
    IWordsDistanceStore
{
    private readonly IDistancesPersistenceCacheAdapter _persistenceAdapter;
    private readonly IWordsCache _wordsCache;

    public CachedWordsDistanceStore(
        IDistancesPersistenceCacheAdapter persistenceAdapter,
        IWordsCache wordsCache)
    {
        _persistenceAdapter = persistenceAdapter ?? throw new ArgumentNullException(nameof(persistenceAdapter));
        _wordsCache = wordsCache ?? throw new ArgumentNullException(nameof(wordsCache));
    }

    public async Task<WordsData> GetData(WordsDataRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var version = await _persistenceAdapter.GetVersion(ct);

        var wordsIndex = await _wordsCache.GetOrCreateWordsIndex(
            version,
            factoryCt => _persistenceAdapter.GetWordIndex(version, factoryCt),
            ct)
        ?? throw new WordDistancesStoreException($"Index for version {version} not found");

        ushort? totalWords = null;
        Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>? distancesToWords = null;
        Result<WordDistance, WordPairDataErrorCode>? distanceToWord = null;
        Result<WordDistance, SourceWordDataErrorCode>? wordByDistance = null;
        Result<IReadOnlyList<Word>, SourceWordDataErrorCode>? closestWords = null;

        if (request.TotalWords is not null)
        {
            totalWords = wordsIndex.TotalWords;
        }

        if (request.DistancesToWords is not null)
        {
            distancesToWords = await GetDistancesToWords(request.DistancesToWords, wordsIndex, version, ct);
        }

        if (request.DistanceToWord is not null)
        {
            distanceToWord = await GetDistanceToWord(request.DistanceToWord, wordsIndex, version, ct);
        }

        if (request.WordByDistance is not null)
        {
            wordByDistance = await GetWordByDistance(request.WordByDistance, wordsIndex, version, ct);
        }

        if (request.ClosestWords is not null)
        {
            closestWords = await GetClosestWords(request.ClosestWords, wordsIndex, version, ct);
        }

        return new WordsData(
            version: version,
            totalWords: totalWords,
            distancesToWords: distancesToWords,
            distanceToWord: distanceToWord,
            wordByDistance: wordByDistance,
            closestWords: closestWords);
    }

    private async Task<Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>> GetDistancesToWords(
        RequestOperation.DistancesToWords operation,
        WordsIndexCacheEntry wordsIndex,
        int version,
        CancellationToken ct)
    {
        if (!wordsIndex.WordIdsByText.TryGetValue(operation.SourceWord.Text, out var sourceWordId))
        {
            return Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var wordSnapshot = await _wordsCache.GetOrCreateWordSnapshot(
            sourceWordId,
            version,
            factoryCt => _persistenceAdapter.GetWordSnapshot(sourceWordId, version, factoryCt),
            ct);

        if (wordSnapshot is null)
        {
            return Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var result = operation.TargetWords
            .Select(word =>
            {
                if (wordsIndex.WordIdsByText.TryGetValue(word.Text, out var wordId))
                {
                    return new NullableDistance(word, wordSnapshot.GetDistanceByWordId(wordId));
                }
                else
                {
                    return new NullableDistance(word, null);
                }
            })
            .ToArray();

        return Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>.Success(result);
    }

    private async Task<Result<WordDistance, WordPairDataErrorCode>> GetDistanceToWord(
        RequestOperation.DistanceToWord operation,
        WordsIndexCacheEntry wordsIndex,
        int version,
        CancellationToken ct)
    {
        if (!wordsIndex.WordIdsByText.TryGetValue(operation.SourceWord.Text, out var sourceWordId))
        {
            return Result<WordDistance, WordPairDataErrorCode>.Failure(
                WordPairDataErrorCode.SourceWordNotFound);
        }

        var wordSnapshot = await _wordsCache.GetOrCreateWordSnapshot(
            sourceWordId,
            version,
            factoryCt => _persistenceAdapter.GetWordSnapshot(sourceWordId, version, factoryCt),
            ct);

        if (wordSnapshot is null)
        {
            return Result<WordDistance, WordPairDataErrorCode>.Failure(
                WordPairDataErrorCode.SourceWordNotFound);
        }

        if (!wordsIndex.WordIdsByText.TryGetValue(operation.TargetWord.Text, out var targetWordId))
        {
            return Result<WordDistance, WordPairDataErrorCode>.Failure(
                WordPairDataErrorCode.TargetWordNotFound);
        }

        var distance = wordSnapshot.GetDistanceByWordId(targetWordId);
        var result = new WordDistance(operation.TargetWord, distance);
        return Result<WordDistance, WordPairDataErrorCode>.Success(result);
    }

    private async Task<Result<WordDistance, SourceWordDataErrorCode>> GetWordByDistance(
        RequestOperation.WordByDistance operation,
        WordsIndexCacheEntry wordsIndex,
        int version,
        CancellationToken ct)
    {
        if (!wordsIndex.WordIdsByText.TryGetValue(operation.SourceWord.Text, out var sourceWordId))
        {
            return Result<WordDistance, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var wordSnapshot = await _wordsCache.GetOrCreateWordSnapshot(
            sourceWordId,
            version,
            factoryCt => _persistenceAdapter.GetWordSnapshot(sourceWordId, version, factoryCt),
            ct);

        if (wordSnapshot is null)
        {
            return Result<WordDistance, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var distance = operation.FuncOfTotalWordsCount(wordsIndex.TotalWords);
        var wordId = wordSnapshot.GetWordIdByDistance(distance);
        var word = wordsIndex.GetWordById(wordId);
        var result = new WordDistance(word, distance);

        return Result<WordDistance, SourceWordDataErrorCode>.Success(result);
    }

    private async Task<Result<IReadOnlyList<Word>, SourceWordDataErrorCode>> GetClosestWords(
        RequestOperation.ClosestWords operation,
        WordsIndexCacheEntry wordsIndex,
        int version,
        CancellationToken ct)
    {
        if (!wordsIndex.WordIdsByText.TryGetValue(operation.SourceWord.Text, out var sourceWordId))
        {
            return Result<IReadOnlyList<Word>, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var wordSnapshot = await _wordsCache.GetOrCreateWordSnapshot(
            sourceWordId,
            version,
            factoryCt => _persistenceAdapter.GetWordSnapshot(sourceWordId, version, factoryCt),
            ct);

        if (wordSnapshot is null)
        {
            return Result<IReadOnlyList<Word>, SourceWordDataErrorCode>.Failure(
                SourceWordDataErrorCode.SourceWordNotFound);
        }

        var words = wordSnapshot.GetClosestWordIds(operation.WordsCount)
            .Select(wordsIndex.GetWordById)
            .ToArray();

        return Result<IReadOnlyList<Word>, SourceWordDataErrorCode>.Success(words);
    }
}
