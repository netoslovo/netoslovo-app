namespace WordoGuessr.Sagas.Dto;

public sealed record UploadWordsVersionSagaHistoryPageDto(
    IReadOnlyCollection<UploadWordsVersionSagaHistoryDto> Uploads,
    bool HasMore);
