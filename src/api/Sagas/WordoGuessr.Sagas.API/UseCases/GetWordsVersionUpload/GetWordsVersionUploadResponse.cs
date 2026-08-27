using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUpload;

public sealed record GetWordsVersionUploadResponse(UploadWordsVersionSagaDto? Upload);
