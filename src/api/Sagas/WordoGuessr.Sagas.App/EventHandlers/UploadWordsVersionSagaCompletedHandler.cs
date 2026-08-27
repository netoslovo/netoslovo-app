using Wolverine.Attributes;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.Domain.UploadWordsVersion.Events;

namespace WordoGuessr.Sagas.App.EventHandlers;

public static class UploadWordsVersionSagaCompletedHandler
{
    [Transactional]
    public static void Handle(UploadWordsVersionSagaCompleted @event, ISagasStore store)
    {
        store.UploadWordsVersionSagasHistory.Add(@event.SagaHistory);
    }
}
