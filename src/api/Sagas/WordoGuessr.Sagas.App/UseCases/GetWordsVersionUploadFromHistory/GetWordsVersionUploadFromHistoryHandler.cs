using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.App.Mapping;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadFromHistory;

internal sealed class GetWordsVersionUploadFromHistoryHandler
    : IQueryHandler<GetWordsVersionUploadFromHistoryQuery, UploadWordsVersionSagaHistoryDto?>
{
    private readonly ISagasStore _store;

    public GetWordsVersionUploadFromHistoryHandler(ISagasStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<UploadWordsVersionSagaHistoryDto?> Handle(
        GetWordsVersionUploadFromHistoryQuery query,
        CancellationToken ct = default)
    {
        var saga = await _store.UploadWordsVersionSagasHistory
            .AsNoTracking()
            .SingleOrDefaultAsync(saga => saga.SagaId == query.SagaId, ct);

        return saga?.MapToDto();
    }
}
