using Wolverine.Attributes;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.App.DomainEventsHandlers;

public static class GameFinishedArcadeGameStatsHandler
{
    [Transactional]
    public static async Task Handle(
        GameFinishedEvent @event,
        IGameReadModelStore gameReadStore,
        CancellationToken ct)
    {
        if (@event.Mode != SingleGameMode.Arcade ||
            @event.State != SingleGameStateCode.Guessed) return;

        await gameReadStore.RecordSuccessfulArcadeGame(
            @event.PlayerId,
            Difficulty.FromCode(@event.DifficultyCode),
            @event.Score,
            @event.Attempts,
            @event.FinishedAt - @event.StartedAt,
            ct
        );
    }
}
