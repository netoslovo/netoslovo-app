using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using JasperFx;
using JasperFx.CodeGeneration;
using Wolverine.Attributes;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public class InsertWordDistanceMapsHandlerOptions : INamedOptions
{
    public static string Name => "Words:InsertWordDistanceMapsHandler";

    [Range(typeof(TimeSpan), "00:00:30", "1.00:00:00")]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class InsertWordDistanceMapsTimeoutAttribute : ModifyChainAttribute
{
    public override void Modify(
        IChain chain,
        GenerationRules rules,
        IServiceContainer container)
    {
        var options = container
            .GetInstance<IOptions<InsertWordDistanceMapsHandlerOptions>>()
            .Value;

        ((HandlerChain)chain).ExecutionTimeoutInSeconds =
            checked((int)Math.Ceiling(options.Timeout.TotalSeconds));
    }
}

[InsertWordDistanceMapsTimeout]
public static class InsertWordDistanceMapsHandler
{
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
