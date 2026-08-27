using Wolverine.Attributes;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.App.DomainEventsHandlers;

public static class GameFinishedDailyStreakEventHandler
{
    [Transactional]
    public static async Task Handle(
        GameFinishedEvent @event,
        IGameReadModelStore gameReadStore,
        CancellationToken ct)
    {
        if (@event.Mode != SingleGameMode.Daily ||
            @event.State != SingleGameStateCode.Guessed ||
            @event.DayOfDailyGame is null ||
            DailyGameClock.GetDateOnly(@event.FinishedAt) != @event.DayOfDailyGame) return;

        await gameReadStore.RecordDailyGameStreakSuccess(
            @event.PlayerId,
            @event.DayOfDailyGame.Value,
            ct);
    }
}
