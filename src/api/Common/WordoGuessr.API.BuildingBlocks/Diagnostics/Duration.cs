using System.Diagnostics;

namespace WordoGuessr.API.BuildingBlocks.Diagnostics;

public readonly struct Duration : IDisposable
{
    private readonly long _startedAt;
    private readonly Action<TimeSpan> _record;

    private Duration(Action<TimeSpan> record)
    {
        _startedAt = Stopwatch.GetTimestamp();
        _record = record;
    }

    public static Duration Measure(Action<TimeSpan> record)
        => new(record);

    public void Dispose()
        => _record(Stopwatch.GetElapsedTime(_startedAt));
}