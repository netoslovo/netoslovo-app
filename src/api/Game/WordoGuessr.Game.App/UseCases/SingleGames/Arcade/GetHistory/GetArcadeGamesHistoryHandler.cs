using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;

internal sealed class GetArcadeGamesHistoryHandler
    : IQueryHandler<GetArcadeGamesHistoryQuery, ArcadeGamesHistoryDto>
{
    private readonly IGameStore _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetArcadeGamesHistoryHandler(
        IGameStore dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ArcadeGamesHistoryDto> Handle(GetArcadeGamesHistoryQuery query, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);
        var gamesFromDb = await _dbContext.SingleGamesForSummary()
            .Where(sg =>
                sg.PlayerId == query.PlayerId &&
                sg.Mode == Domain.SingleGameMode.Arcade)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .ToArrayAsync(ct);

        var hasMore = gamesFromDb.Length > query.Take;
        var games = gamesFromDb
            .Take(query.Take)
            .Select(game => game.MapToArcadeGameInfoDto(today))
            .ToArray();

        return new ArcadeGamesHistoryDto(games, hasMore);
    }
}
