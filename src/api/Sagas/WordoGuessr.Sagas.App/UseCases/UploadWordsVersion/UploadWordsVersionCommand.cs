using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Sagas.App.UseCases.UploadWordsVersion;

public sealed record UploadWordsVersionCommand(Guid SagaId, int Version)
    : ICommand<Result<UploadWordsVersionError>>;
