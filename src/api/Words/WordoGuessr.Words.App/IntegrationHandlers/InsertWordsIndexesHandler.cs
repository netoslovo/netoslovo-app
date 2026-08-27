using System.Globalization;
using CsvHelper;
using Wolverine.Attributes;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class InsertWordsIndexHandler
{
    [MessageTimeout(10 * 60)]
    public static async Task<WordsIndexesInserted> Handle(
        InsertWordsIndexes command,
        IWordsDistanceStoreLoader wordsDistanceStoreLoader,
        IObjectStorage objectStorage,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        const string wordsKey = "words.csv";
        var wordsPath = ObjectStorageHelpers.GetFullPathForKey(command.WordsVersion, wordsKey);

        await using var wordsDownload = await objectStorage.GetAsync(wordsPath, ct);
        using var streamReader = new StreamReader(wordsDownload.Content);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var enumerable = csvReader
            .GetRecordsAsync<WordCsvItem>(ct)
            .Select(i => new WordIndex(command.WordsVersion, i.Id, i.Text));

        var totalWordsCount = await wordsDistanceStoreLoader.InsertWordsIndexes(enumerable, command.WordsVersion, ct);

        return new WordsIndexesInserted(command.SagaId, command.WordsVersion, totalWordsCount, timeProvider.GetUtcNow());
    }
    private sealed record WordCsvItem(ushort Id, string Text);
}
