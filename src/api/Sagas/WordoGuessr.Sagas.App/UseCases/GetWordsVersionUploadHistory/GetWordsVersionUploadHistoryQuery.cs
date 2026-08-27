using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadHistory;

public sealed record GetWordsVersionUploadHistoryQuery(
    int Skip,
    int Take,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int? WordsVersion,
    UploadWordsVersionSagaHistorySortFieldDto? SortBy,
    SortDirectionDto? SortDirection)
    : IQuery<UploadWordsVersionSagaHistoryPageDto>;
