using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Email.Infra.EmailSending;

internal sealed class SmtpMetrics
{
    private readonly Histogram<double> _sendDuration;
    private readonly Histogram<double> _lockWaitDuration;
    private readonly Counter<long> _timeouts;
    private readonly Counter<long> _clientInvalidations;
    private readonly Counter<long> _clientConnections;
    private readonly Counter<long> _clientConnectionErrors;

    public SmtpMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Smtp");

        _sendDuration = meter.CreateHistogram(
            "smtp.send.duration",
            unit: "s",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    0.01,
                    0.025,
                    0.05,
                    0.1,
                    0.25,
                    0.5,
                    1,
                    2.5,
                    5,
                    10,
                    30
                ]
            }
        );

        _lockWaitDuration = meter.CreateHistogram(
            "smtp.client.lock_wait.duration",
            unit: "s",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    0.001,
                    0.005,
                    0.01,
                    0.025,
                    0.05,
                    0.1,
                    0.25,
                    0.5,
                    1,
                    2.5,
                    5,
                    10,
                    30
                ]
            });

        _timeouts = meter.CreateCounter<long>(
            "smtp.timeout");

        _clientInvalidations = meter.CreateCounter<long>(
            "smtp.client.invalidations");

        _clientConnections = meter.CreateCounter<long>(
            "smtp.client.connections");

        _clientConnectionErrors = meter.CreateCounter<long>(
            "smtp.client.connection.errors");
    }

    public void RecordSendDuration(TimeSpan duration)
    {
        _sendDuration.Record(duration.TotalSeconds);
    }

    public void RecordLockWaitDuration(TimeSpan duration)
    {
        _lockWaitDuration.Record(duration.TotalSeconds);
    }

    public void ClientInvalidated()
    {
        _clientInvalidations.Add(1);
    }

    public void ClientConnected()
    {
        _clientConnections.Add(1);
    }

    public void ConnectionErrorOccurred()
    {
        _clientConnectionErrors.Add(1);
    }

    public void TimeoutOccurred(TimeoutOperation operation)
    {
        _timeouts.Add(
            1,
            new KeyValuePair<string, object?>(
                "operation",
                operation.ToString()
            )
        );
    }

    public enum TimeoutOperation
    {
        Lock,
        Send,
        NoOp,
        Connect
    }
}