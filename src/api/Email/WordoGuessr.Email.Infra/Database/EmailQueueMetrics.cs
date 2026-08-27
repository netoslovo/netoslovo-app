using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Email.Infra.Database;

internal sealed class EmailQueueMetrics
{
    private readonly Gauge<int> _queueReadyMessages;
    private readonly Gauge<double> _queueOldestReadyAge;
    private readonly Gauge<int> _queueTotalMessages;

    public EmailQueueMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("EmailQueue");
        _queueReadyMessages = meter.CreateGauge<int>("email.queue.ready");
        _queueOldestReadyAge = meter.CreateGauge<double>(
            "email.queue.oldest_ready.age",
            unit: "s");
        _queueTotalMessages = meter.CreateGauge<int>("email.queue.total");
    }

    public void RecordReadyMessagesCount(int readyMessagesCount)
    {
        _queueReadyMessages.Record(readyMessagesCount);
    }

    public void RecordOldestReadyAge(TimeSpan oldestReadyAge)
    {
        _queueOldestReadyAge.Record(Math.Max(0, oldestReadyAge.TotalSeconds));
    }

    public void RecordTotalMessagesCount(int totalMessagesCount)
    {
        _queueTotalMessages.Record(totalMessagesCount);
    }
}
