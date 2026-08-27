using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.App.DomainEventsHandlers;

public static class GameCreatedMetricsEventHandler
{
    public static void Handle(GameCreatedEvent @event, GameAppMetrics metrics)
    {
        metrics.GameCreated(@event.Mode, @event.DifficultyCode);
    }
}
