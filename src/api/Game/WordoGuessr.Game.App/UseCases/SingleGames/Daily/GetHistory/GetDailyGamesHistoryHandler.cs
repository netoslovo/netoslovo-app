using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetHistory;

internal sealed class GetDailyGamesHistoryHandler
    : IQueryHandler<GetDailyGamesHistoryQuery, DailyGamesHistoryDto>
{
    private readonly IGameStore _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;

    public GetDailyGamesHistoryHandler(
        IGameStore dbContext,
        TimeProvider timeProvider,
        DisplayWordDtoBuilder displayWordDtoBuilder)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
    }

    public async Task<DailyGamesHistoryDto> Handle(GetDailyGamesHistoryQuery query, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var gamesFromDb = await _dbContext.SingleGamesDailySchedules
            .AsNoTracking()
            .Where(s => s.Day <= today)
            .OrderByDescending(s => s.Day)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .Select(dc => new
            {
                dc.Day,
                Game = _dbContext.DailySingleGamesForSummary()
                    .Where(sg =>
                        sg.PlayerId == query.PlayerId &&
                        sg.VersionedGameSource.GameSourceId == dc.ApprovedGameSourceId
                    )
                    .FirstOrDefault()
            })
            .ToArrayAsync(ct);

        var hasMore = gamesFromDb.Length > query.Take;

        var games = gamesFromDb
            .Take(query.Take)
            .Select(item => item.Game is null
                ? SingleGameInfoMapping.MapToMissingDailyGameInfoDto(item.Day, today)
                : item.Game.MapToDailyGameInfoDto(
                    item.Day,
                    today,
                    _displayWordDtoBuilder.Build(item.Game)))
            .ToArray();

        return new DailyGamesHistoryDto(games, hasMore);
    }
}
