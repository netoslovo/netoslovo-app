using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.API.UseCases.GetWordsVersionUploadFromHistory;

public sealed record GetWordsVersionUploadFromHistoryResponse(UploadWordsVersionSagaHistoryDto? Upload);
