using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilterMetrics
{
    private readonly Gauge<int> _appVersion;
    private readonly Gauge<int> _storedVersion;

    public UserNameFilterMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("UserNameFiltration");

        _appVersion = meter.CreateGauge<int>("username_filter.version.app");
        _appVersion.Record(-1);

        _storedVersion = meter.CreateGauge<int>("username_filter.version.stored");
        _storedVersion.Record(-1);
    }

    public void SetAppVersion(int version)
    {
        _appVersion.Record(version);
    }

    public void SetStoredVersion(int version)
    {
        _storedVersion.Record(version);
    }
}