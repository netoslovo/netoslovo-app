namespace WordoGuessr.Sagas.Dto;

public sealed record UploadWordsVersionSagaHistoryDto(
    Guid SagaId,
    int WordsVersion,
    UploadWordsVersionSagaStateDto State,
    UploadWordsVersionSagaStepNameDto CurrentStep,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyCollection<UploadWordsVersionSagaCompletedStepDto> CompletedSteps,
    int Version);
