using WordoGuessr.Common.Domain;

namespace WordoGuessr.Sagas.Domain.UploadWordsVersion.Events;

public sealed record UploadWordsVersionSagaCompleted(UploadWordsVersionSagaHistory SagaHistory)
    : IDomainEvent;
