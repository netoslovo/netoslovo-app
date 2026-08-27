using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App;

public sealed class GameAppMetrics
{
    private readonly Counter<long> _created;
    private readonly Counter<long> _finished;

    public GameAppMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Game.App");
        _created = meter.CreateCounter<long>("game.games.created");
        _finished = meter.CreateCounter<long>("game.games.finished");
    }

    public void GameCreated(SingleGameMode mode, string difficultyCode) =>
        _created.Add(1,
            new KeyValuePair<string, object?>("mode", mode.ToString()),
            new KeyValuePair<string, object?>("difficulty", difficultyCode));

    public void GameFinished(SingleGameMode mode, string difficultyCode, SingleGameStateCode state) =>
        _finished.Add(1,
            new KeyValuePair<string, object?>("mode", mode.ToString()),
            new KeyValuePair<string, object?>("difficulty", difficultyCode),
            new KeyValuePair<string, object?>("state", state.ToString()));
}
