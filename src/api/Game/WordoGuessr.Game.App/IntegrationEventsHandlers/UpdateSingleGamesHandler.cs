using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Contract.Commands;
using WordoGuessr.Game.Contract.Events;

namespace WordoGuessr.Game.App.IntegrationEventsHandlers;

public static class UpdateSingleGamesHandler
{
    // TODO
    public static SingleGamesUpdated Handle(UpdateSingleGames command, IGameStore gameStore, TimeProvider timeProvider)
    {
        return new SingleGamesUpdated(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }

}
