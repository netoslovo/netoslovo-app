using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploads;

public sealed record GetWordsVersionUploadsQuery(int Skip, int Take)
    : IQuery<UploadWordsVersionSagaPageDto>;
