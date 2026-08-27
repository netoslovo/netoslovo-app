using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.App.Mapping;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploads;

internal sealed class GetWordsVersionUploadsHandler
    : IQueryHandler<GetWordsVersionUploadsQuery, UploadWordsVersionSagaPageDto>
{
    private readonly ISagasStore _store;

    public GetWordsVersionUploadsHandler(ISagasStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<UploadWordsVersionSagaPageDto> Handle(
        GetWordsVersionUploadsQuery query,
        CancellationToken ct = default)
    {
        var uploadsPlusOne = await _store.UploadWordsVersionSagas
            .AsNoTracking()
            .OrderBy(saga => saga.CreatedAt)
            .ThenBy(saga => saga.SagaId)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .ToArrayAsync(ct);

        var hasMore = uploadsPlusOne.Length > query.Take;
        var uploads = uploadsPlusOne
            .Take(query.Take)
            .Select(saga => saga.MapToDto())
            .ToArray();

        return new UploadWordsVersionSagaPageDto(uploads, hasMore);
    }
}
