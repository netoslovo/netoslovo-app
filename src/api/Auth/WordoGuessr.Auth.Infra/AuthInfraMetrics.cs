using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Auth.Infra;

internal sealed class AuthInfraMetrics
{
    private readonly Gauge<int> _totalUsers;

    public AuthInfraMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Auth.Infra");
        _totalUsers = meter.CreateGauge<int>("auth.users.total");
    }

    public void RecordTotalUsers(int count) => _totalUsers.Record(count);
}