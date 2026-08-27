using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.App.Mapping;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUpload;

internal sealed class GetWordsVersionUploadHandler
    : IQueryHandler<GetWordsVersionUploadQuery, UploadWordsVersionSagaDto?>
{
    private readonly ISagasStore _store;

    public GetWordsVersionUploadHandler(ISagasStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<UploadWordsVersionSagaDto?> Handle(
        GetWordsVersionUploadQuery query,
        CancellationToken ct = default)
    {
        var saga = await _store.UploadWordsVersionSagas
            .AsNoTracking()
            .SingleOrDefaultAsync(saga => saga.SagaId == query.SagaId, ct);

        return saga?.MapToDto();
    }
}
