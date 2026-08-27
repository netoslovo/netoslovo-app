using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetDailySourceReviews;

internal sealed class GetDailySourceReviewsHandler
    : IQueryHandler<
        GetDailySourceReviewsQuery,
        Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;

    public GetDailySourceReviewsHandler(IGameStore dbContext, IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>> Handle(
        GetDailySourceReviewsQuery query,
        CancellationToken ct)
    {
        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var dbQuery = _dbContext.DailyGameSourceReviews
            .AsNoTracking()
            .Include(r => r.ApprovedSource)
                .ThenInclude(s => s!.Schedule)
            .Include(r => r.VersionedGameSource)
                .ThenInclude(gs => gs.GameSource)
            .AsQueryable();

        if (query.DifficultyCodeFilter is not null)
        {
            var validDifficulty = Difficulty.TryFromCode(query.DifficultyCodeFilter, out var difficulty);
            if (!validDifficulty)
            {
                return Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>.Failure(GetDailySourceReviewsError.InvalidDifficulty);
            }

            dbQuery = dbQuery.Where(r => r.VersionedGameSource.Difficulty == difficulty);
        }

        if (query.ExistsInLatestVersionFilter is not null)
        {
            dbQuery = query.ExistsInLatestVersionFilter.Value
                ? dbQuery.Where(r => _dbContext.VersionedGameSources.Any(vgs =>
                    vgs.GameSourceId == r.GameSourceId &&
                    vgs.WordsVersion == wordsVersion))
                : dbQuery.Where(r => !_dbContext.VersionedGameSources.Any(vgs =>
                    vgs.GameSourceId == r.GameSourceId &&
                    vgs.WordsVersion == wordsVersion));
        }

        if (query.WordFilter is not null)
        {
            var validWord = Word.TryCreate(query.WordFilter, out var word);
            if (!validWord)
            {
                return Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>.Failure(GetDailySourceReviewsError.InvalidWord);
            }

            dbQuery = dbQuery.Where(r => r.VersionedGameSource.GameSource.Word == word);
        }

        if (query.ApprovedFilter is not null)
        {
            dbQuery = query.ApprovedFilter.Value
                ? dbQuery.Where(r => r.ApprovedSource != null)
                : dbQuery.Where(r => r.ApprovedSource == null);
        }

        if (query.ScheduledFilter is not null)
        {
            dbQuery = query.ScheduledFilter.Value
                ? dbQuery.Where(r => r.ApprovedSource != null && r.ApprovedSource.Schedule != null)
                : dbQuery.Where(r => r.ApprovedSource == null || r.ApprovedSource.Schedule == null);
        }

        if (query.SortBy.HasValue)
        {
            var direction = query.SortDirection ?? SortDirectionDto.Desc;

            dbQuery = (query.SortBy.Value, direction) switch
            {
                (GameSourceReviewSortFieldDto.CreatedAt, SortDirectionDto.Asc) => dbQuery.OrderBy(r => r.CreatedAt).ThenBy(r => r.GameSourceId),
                (GameSourceReviewSortFieldDto.CreatedAt, SortDirectionDto.Desc) => dbQuery.OrderByDescending(r => r.CreatedAt).ThenBy(r => r.GameSourceId),
                (GameSourceReviewSortFieldDto.UpdatedAt, SortDirectionDto.Asc) => dbQuery.OrderBy(r => r.UpdatedAt).ThenBy(r => r.GameSourceId),
                (GameSourceReviewSortFieldDto.UpdatedAt, SortDirectionDto.Desc) => dbQuery.OrderByDescending(r => r.UpdatedAt).ThenBy(r => r.GameSourceId),
                (GameSourceReviewSortFieldDto.GameSourceId, SortDirectionDto.Asc) => dbQuery.OrderBy(r => r.GameSourceId),
                (GameSourceReviewSortFieldDto.GameSourceId, SortDirectionDto.Desc) => dbQuery.OrderByDescending(r => r.GameSourceId),
                _ => dbQuery
            };
        }
        else
        {
            dbQuery = dbQuery
                .OrderByDescending(r => r.UpdatedAt)
                .ThenBy(r => r.GameSourceId);
        }

        dbQuery = dbQuery
            .Skip(query.Skip)
            .Take(query.Take + 1);

        var reviewsFromDb = await dbQuery.ToListAsync(ct);

        var hasMore = reviewsFromDb.Count > query.Take;
        var pageReviews = reviewsFromDb
            .Take(query.Take)
            .ToArray();
        var pageGameSourceIds = pageReviews
            .Select(r => r.GameSourceId)
            .ToArray();
        var latestGameSourceIds = await _dbContext.VersionedGameSources
            .AsNoTracking()
            .Where(vgs =>
                vgs.WordsVersion == wordsVersion &&
                pageGameSourceIds.Contains(vgs.GameSourceId))
            .Select(vgs => vgs.GameSourceId)
            .ToHashSetAsync(ct);

        var reviews = pageReviews
            .Select(r => new DailyGameSourceReviewDto(
                GameSource: r.VersionedGameSource.MapToDto(),
                ExistsInLatestVersion: latestGameSourceIds.Contains(r.GameSourceId),
                Approved: r.ApprovedSource is not null,
                Scheduled: r.ApprovedSource is not null && r.ApprovedSource.Schedule is not null,
                CreatedAt: r.CreatedAt,
                UpdatedAt: r.UpdatedAt
            ))
            .ToArray();

        return Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>
            .Success(new DailyGameSourceReviewsDto(reviews, hasMore));
    }
}
