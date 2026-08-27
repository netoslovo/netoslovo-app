using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.App.DomainEventsHandlers;

public static class GameFinishedMetricsEventHandler
{
    public static void Handle(GameFinishedEvent @event, GameAppMetrics metrics)
    {
        metrics.GameFinished(@event.Mode, @event.DifficultyCode, @event.State);
    }
}
