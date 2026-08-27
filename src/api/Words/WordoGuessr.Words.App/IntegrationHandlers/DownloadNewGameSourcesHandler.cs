using System.Globalization;
using CsvHelper;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;
using WordoGuessr.Common.Dto;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class DownloadNewGameSourcesHandler
{
    public static async Task<NewGameSourcesDownloaded> Handle(
        DownloadNewGameSources command,
        IObjectStorage objectStorage,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        const string gameSourcesKey = "game-sources.csv";
        var gameSourcesPath = ObjectStorageHelpers.GetFullPathForKey(command.WordsVersion, gameSourcesKey);

        await using var gameSourcesDownload = await objectStorage.GetAsync(gameSourcesPath, ct);
        using var streamReader = new StreamReader(gameSourcesDownload.Content);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var gameSources = await csvReader
            .GetRecordsAsync<GameSourceCsvItem>(ct)
            .Select(i => new GameSourceDto(i.GameSourceId, i.Word, i.DifficultyCode, i.PredictedDifficulty))
            .ToArrayAsync(ct);

        return new NewGameSourcesDownloaded(command.SagaId, command.WordsVersion, gameSources, timeProvider.GetUtcNow());
    }

    private sealed record GameSourceCsvItem(
        int GameSourceId,
        string Word,
        string DifficultyCode,
        int PredictedDifficulty);
}
