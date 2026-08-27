namespace WordoGuessr.Sagas.Dto;

public sealed record UploadWordsVersionSagaDto(
    Guid SagaId,
    int WordsVersion,
    UploadWordsVersionSagaStateDto State,
    UploadWordsVersionSagaStepNameDto CurrentStep,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyCollection<UploadWordsVersionSagaCompletedStepDto> CompletedSteps,
    int Version);
