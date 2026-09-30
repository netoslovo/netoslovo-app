using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetToday;

internal sealed class GetTodayDailyGameHandler
    : IQueryHandler<GetTodayDailyGameQuery, Result<DailyGameInfoDto?, GetDailyError>>
{
    private readonly IGameStore _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetTodayDailyGameHandler(
        IGameStore dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Result<DailyGameInfoDto?, GetDailyError>> Handle(GetTodayDailyGameQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var todayGameSource = await _dbContext.SingleGamesDailySchedules
            .Where(s => s.Day == today)
            .Select(s => new { Id = s.ApprovedGameSourceId })
            .FirstOrDefaultAsync(ct);

        if (todayGameSource is null)
        {
            return Result<DailyGameInfoDto?, GetDailyError>.Failure(GetDailyError.ScheduleNotFound);
        }

        var game = await _dbContext.SingleGamesForSummary()
            .Where(sg => sg.PlayerId == query.PlayerId &&
                sg.Mode == SingleGameMode.Daily &&
                sg.VersionedGameSource.GameSourceId == todayGameSource.Id)
            .FirstOrDefaultAsync(ct);

        if (game is null)
        {
            return Result<DailyGameInfoDto?, GetDailyError>.Success(null);
        }

        var dto = game.MapToDailyGameInfoDto(today, today);

        return Result<DailyGameInfoDto?, GetDailyError>.Success(dto);
    }
}
