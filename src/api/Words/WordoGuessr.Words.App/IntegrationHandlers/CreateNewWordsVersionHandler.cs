using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class CreateNewWordsVersionHandler
{
    public static async Task<NewWordsVersionCreated> Handle(
        CreateNewWordsVersion command,
        IWordsDistanceStoreLoader wordsDistanceStoreLoader,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        await wordsDistanceStoreLoader.InsertWordsVersion(command.WordsVersion, ct);
        return new NewWordsVersionCreated(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }
}
