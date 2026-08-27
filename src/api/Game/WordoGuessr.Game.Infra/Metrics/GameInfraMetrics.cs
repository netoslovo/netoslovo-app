using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Game.Infra.Metrics;

internal sealed class GameInfraMetrics
{
    private readonly Gauge<int> _activePlayers;

    public GameInfraMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Game.Infra");
        _activePlayers = meter.CreateGauge<int>("game.players.active");
    }

    public void RecordActivePlayers(int count) => _activePlayers.Record(count);
}
