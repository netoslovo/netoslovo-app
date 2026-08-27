using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class ActivateNewWordsVersionHandler
{
    public static async Task<NewWordsVersionActivated> Handle(
        ActivateNewWordsVersion command,
        IWordsDistanceStoreLoader wordsDistanceStoreLoader,
        TimeProvider timeProvider,
        CancellationToken ct = default)
    {
        await wordsDistanceStoreLoader.ActivateWordsVersion(command.WordsVersion, ct);
        return new NewWordsVersionActivated(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }
}
