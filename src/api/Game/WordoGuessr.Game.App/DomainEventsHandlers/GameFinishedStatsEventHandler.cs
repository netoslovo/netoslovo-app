using Wolverine.Attributes;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain.SingleGameEvents;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.App.DomainEventsHandlers;

public static class GameFinishedStatsEventHandler
{
    [Transactional]
    public static Task Handle(GameFinishedEvent @event, IGameReadModelStore gameReadStore)
    {
        var gameResult = SingleGameResult.FromGameFinishedEvent(@event);
        return gameReadStore.AddGameResult(gameResult);
    }
}
