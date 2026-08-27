namespace WordoGuessr.Sagas.Dto;

public sealed record UploadWordsVersionSagaPageDto(
    IReadOnlyCollection<UploadWordsVersionSagaDto> Uploads,
    bool HasMore);
