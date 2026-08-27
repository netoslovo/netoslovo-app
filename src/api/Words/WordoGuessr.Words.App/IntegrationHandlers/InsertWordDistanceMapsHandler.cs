using System.Globalization;
using System.Runtime.CompilerServices;
using Wolverine.Attributes;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class InsertWordDistanceMapsHandler
{
    [MessageTimeout(10 * 60)]
    public static async Task<WordDistanceMapsInserted> Handle(
        InsertWordDistanceMaps command,
        IWordsDistanceStoreLoader wordsDistanceStoreLoader,
        IObjectStorage objectStorage,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        long validFileLength = (long)command.TotalWordsCount * command.TotalWordsCount * sizeof(ushort);

        const string wordsByDistancesMapKey = "words-by-distances.bin";
        var wordsByDistancesPath = ObjectStorageHelpers.GetFullPathForKey(command.WordsVersion, wordsByDistancesMapKey);
        using var wordsByDistancesDownload = await objectStorage.GetAsync(wordsByDistancesPath, ct);
        ValidateDownloadedDistances(wordsByDistancesDownload, wordsByDistancesPath, validFileLength);

        const string distancesByWordsMapKey = "distances-by-words.bin";
        var distancesByWordsPath = ObjectStorageHelpers.GetFullPathForKey(command.WordsVersion, distancesByWordsMapKey);
        using var distancesByWordsDownload = await objectStorage.GetAsync(distancesByWordsPath, ct);
        ValidateDownloadedDistances(distancesByWordsDownload, distancesByWordsPath, validFileLength);

        var enumerable = BuildDistanceMapsEnumerable(
            wordsByDistancesDownload,
            distancesByWordsDownload,
            command.WordsVersion,
            command.TotalWordsCount,
            ct);

        await wordsDistanceStoreLoader.InsertWordDistanceMaps(enumerable, command.WordsVersion, ct);

        return new WordDistanceMapsInserted(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }

    private static void ValidateDownloadedDistances(
        ObjectDownload? download,
        string objectPath,
        long validLength)
    {
        if (download is null)
        {
            throw new InvalidDataException($"Object '{objectPath}' was not downloaded.");
        }

        if (download.ContentLength != validLength)
        {
            var actualLength = download.ContentLength?.ToString() ?? "unknown";
            throw new InvalidDataException(
                $"Object '{objectPath}' has invalid length. Expected {validLength}, actual {actualLength}.");
        }
    }

    private static async IAsyncEnumerable<WordDistanceMap> BuildDistanceMapsEnumerable(
        ObjectDownload wordsByDistancesDownload,
        ObjectDownload distancesByWordsDownload,
        int version,
        int totalWordsCount,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        long rowSize = totalWordsCount * sizeof(ushort);
        var wordsByDistancesRow = new byte[rowSize];
        var distancesByWordsRow = new byte[rowSize];

        for (int wordId = 0; wordId < totalWordsCount; wordId++)
        {
            await wordsByDistancesDownload.Content.ReadExactlyAsync(wordsByDistancesRow, ct);
            await distancesByWordsDownload.Content.ReadExactlyAsync(distancesByWordsRow, ct);

            var wordDistanceMap = new WordDistanceMap(version, wordId, wordsByDistancesRow, distancesByWordsRow);
            yield return wordDistanceMap;
        }
    }

}
