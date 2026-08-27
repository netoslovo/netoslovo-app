using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class UnloadOldWordsVersionsHandler
{
    public static async Task<OldWordsVersionsUnloaded> Handle(
        UnloadOldWordsVersions command,
        IWordsDistanceStoreLoader wordsDistanceStoreLoader,
        TimeProvider timeProvider,
        CancellationToken ct = default)
    {
        await wordsDistanceStoreLoader.DeleteOldWordsVersions(command.WordsVersion, ct);
        return new OldWordsVersionsUnloaded(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }
}
