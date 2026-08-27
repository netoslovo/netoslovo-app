using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.App.Mapping;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadHistory;

internal sealed class GetWordsVersionUploadHistoryHandler
    : IQueryHandler<GetWordsVersionUploadHistoryQuery, UploadWordsVersionSagaHistoryPageDto>
{
    private readonly ISagasStore _store;

    public GetWordsVersionUploadHistoryHandler(ISagasStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<UploadWordsVersionSagaHistoryPageDto> Handle(
        GetWordsVersionUploadHistoryQuery query,
        CancellationToken ct = default)
    {
        var dbQuery = _store.UploadWordsVersionSagasHistory
            .AsNoTracking();

        if (query.From is not null)
        {
            dbQuery = dbQuery.Where(saga => saga.CreatedAt >= query.From.Value);
        }

        if (query.To is not null)
        {
            dbQuery = dbQuery.Where(saga => saga.CreatedAt <= query.To.Value);
        }

        if (query.WordsVersion is not null)
        {
            dbQuery = dbQuery.Where(saga => saga.WordsVersion == query.WordsVersion.Value);
        }

        dbQuery = Sort(dbQuery, query.SortBy, query.SortDirection);

        var uploadsPlusOne = await dbQuery
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .ToArrayAsync(ct);

        var hasMore = uploadsPlusOne.Length > query.Take;
        var uploads = uploadsPlusOne
            .Take(query.Take)
            .Select(saga => saga.MapToDto())
            .ToArray();

        return new UploadWordsVersionSagaHistoryPageDto(uploads, hasMore);
    }

    private static IQueryable<UploadWordsVersionSagaHistory> Sort(
        IQueryable<UploadWordsVersionSagaHistory> query,
        UploadWordsVersionSagaHistorySortFieldDto? sortBy,
        SortDirectionDto? sortDirection)
    {
        var field = sortBy ?? UploadWordsVersionSagaHistorySortFieldDto.CreatedAt;
        var direction = sortDirection ?? SortDirectionDto.Desc;

        return (field, direction) switch
        {
            (UploadWordsVersionSagaHistorySortFieldDto.CreatedAt, SortDirectionDto.Asc) =>
                query.OrderBy(saga => saga.CreatedAt).ThenBy(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.CreatedAt, SortDirectionDto.Desc) =>
                query.OrderByDescending(saga => saga.CreatedAt).ThenByDescending(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.CompletedAt, SortDirectionDto.Asc) =>
                query.OrderBy(saga => saga.CompletedAt).ThenBy(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.CompletedAt, SortDirectionDto.Desc) =>
                query.OrderByDescending(saga => saga.CompletedAt).ThenByDescending(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.WordsVersion, SortDirectionDto.Asc) =>
                query.OrderBy(saga => saga.WordsVersion).ThenBy(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.WordsVersion, SortDirectionDto.Desc) =>
                query.OrderByDescending(saga => saga.WordsVersion).ThenByDescending(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.State, SortDirectionDto.Asc) =>
                query.OrderBy(saga => saga.State).ThenBy(saga => saga.SagaId),

            (UploadWordsVersionSagaHistorySortFieldDto.State, SortDirectionDto.Desc) =>
                query.OrderByDescending(saga => saga.State).ThenByDescending(saga => saga.SagaId),

            _ =>
                query.OrderByDescending(saga => saga.CreatedAt).ThenByDescending(saga => saga.SagaId)
        };
    }
}
