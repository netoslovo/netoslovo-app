using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Sagas.App.UseCases.CancelWordsVersionUpload;

public sealed record CancelWordsVersionUploadCommand(Guid SagaId)
    : ICommand<Result<CancelWordsVersionUploadError>>;
