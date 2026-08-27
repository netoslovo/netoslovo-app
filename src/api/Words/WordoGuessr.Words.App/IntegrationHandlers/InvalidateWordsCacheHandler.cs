using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Words.App.IntegrationHandlers;

public static class InvalidateWordsCacheHandler
{
    public static async Task<WordsCacheInvalidated> Handle(
        InvalidateWordsCache command,
        IWordsCacheInvalidator cacheInvalidator,
        TimeProvider timeProvider)
    {
        await cacheInvalidator.RemoveVersions(command.WordsVersion);
        return new WordsCacheInvalidated(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }

}
