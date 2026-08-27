using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;

internal sealed class GetDailyGameCurrentPlayerStreakHandler
    : IQueryHandler<GetDailyGameCurrentPlayerStreakQuery, DailyGameStreakDto>
{
    private readonly IGameReadModelStore _gameReadModelStore;
    private readonly TimeProvider _timeProvider;

    public GetDailyGameCurrentPlayerStreakHandler(IGameReadModelStore gameReadModelStore, TimeProvider timeProvider)
    {
        _gameReadModelStore = gameReadModelStore ?? throw new ArgumentNullException(nameof(gameReadModelStore));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<DailyGameStreakDto> Handle(GetDailyGameCurrentPlayerStreakQuery query, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);
        var streak = await _gameReadModelStore
            .GetDailyGameCurrentPlayerStreakValue(query.PlayerId, today, ct);

        var tier = DailyGameStreakTierCalculator.Calculate(streak);
        return new DailyGameStreakDto(streak, tier);
    }
}