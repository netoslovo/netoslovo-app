using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetDailySourceReviews;

public sealed record GetDailySourceReviewsQuery(
    int Skip,
    int Take,
    GameSourceReviewSortFieldDto? SortBy,
    SortDirectionDto? SortDirection,
    string? DifficultyCodeFilter,
    bool? ExistsInLatestVersionFilter,
    bool? ApprovedFilter,
    string? WordFilter,
    bool? ScheduledFilter)
    : IQuery<Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>>;
