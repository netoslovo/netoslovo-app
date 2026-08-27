namespace WordoGuessr.Sagas.Dto;

public sealed record UploadWordsVersionSagaCompletedStepDto(
    UploadWordsVersionSagaStepNameDto Name,
    bool IsSuccess,
    DateTimeOffset CompletedAt);
