using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadFromHistory;

public sealed record GetWordsVersionUploadFromHistoryQuery(Guid SagaId)
    : IQuery<UploadWordsVersionSagaHistoryDto?>;
