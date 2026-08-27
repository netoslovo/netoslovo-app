using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUpload;

public sealed record GetWordsVersionUploadQuery(Guid SagaId)
    : IQuery<UploadWordsVersionSagaDto?>;
