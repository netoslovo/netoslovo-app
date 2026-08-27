namespace WordoGuessr.Words.App.Abstractions;

public interface IWordsDistanceStoreLoader
{
    Task InsertWordsVersion(int version, CancellationToken ct = default);
    Task ActivateWordsVersion(int version, CancellationToken ct = default);
    Task DeleteOldWordsVersions(int upToVersion, CancellationToken ct = default);

    Task<int> InsertWordsIndexes(
        IAsyncEnumerable<WordIndex> wordsIndexes,
        int version,
        CancellationToken ct = default);

    Task InsertWordDistanceMaps(
        IAsyncEnumerable<WordDistanceMap> wordDistanceMaps,
        int version,
        CancellationToken ct = default);
}
