using System.Diagnostics.Metrics;

namespace WordoGuessr.API.BuildingBlocks.Diagnostics;

public static class AppMeterFactory
{
    public const string Prefix = "WordoGuessr";

    public static Meter CreatePrefixedMeter(
        this IMeterFactory meterFactory,
        string meterName)
    {
        return meterFactory.Create($"{Prefix}.{meterName}");
    }
}