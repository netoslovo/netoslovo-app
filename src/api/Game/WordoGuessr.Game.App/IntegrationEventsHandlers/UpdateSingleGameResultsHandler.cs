using WordoGuessr.Game.Contract.Commands;
using WordoGuessr.Game.Contract.Events;

namespace WordoGuessr.Game.App.IntegrationEventsHandlers;

public static class UpdateSingleGameResultsHandler
{
    // TODO
    public static SingleGameResultsUpdated Handle(UpdateSingleGameResults command, TimeProvider timeProvider)
    {
        return new SingleGameResultsUpdated(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }

}
