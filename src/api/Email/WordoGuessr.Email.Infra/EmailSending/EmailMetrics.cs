using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Email.Infra.EmailSending;

public sealed class EmailMetrics
{
    private readonly Counter<long> _enqueued;
    private readonly Counter<long> _duplicates;
    private readonly Counter<long> _sent;
    private readonly Counter<long> _sendFailed;
    private readonly Counter<long> _ackFailed;
    private readonly Counter<long> _expired;
    private readonly Counter<long> _sendAttemptsExhausted;
    private readonly Counter<long> _sendingStale;
    private readonly Histogram<int> _sendAttemptsToSuccess;
    private readonly Histogram<double> _messageLatency;

    public EmailMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Email");
        _enqueued = meter.CreateCounter<long>("email.enqueued");
        _duplicates = meter.CreateCounter<long>("email.duplicates");
        _sent = meter.CreateCounter<long>("email.send.success");
        _sendFailed = meter.CreateCounter<long>("email.send.errors");
        _ackFailed = meter.CreateCounter<long>("email.ack.errors");
        _expired = meter.CreateCounter<long>("email.expired");
        _sendAttemptsExhausted = meter.CreateCounter<long>("email.send.attempts_exhausted");
        _sendingStale = meter.CreateCounter<long>("email.sending.stale");
        _sendAttemptsToSuccess = meter.CreateHistogram(
            "email.send.attempts_to_success",
            advice: new InstrumentAdvice<int>
            {
                HistogramBucketBoundaries =
                [
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9,
                    10,
                    11,
                    12,
                    13,
                    14,
                    15,
                    16,
                    17,
                    18,
                    19,
                    20
                ]
            });

        _messageLatency = meter.CreateHistogram(
            "email.message.latency",
            unit: "s",
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                [
                    1,
                    2.5,
                    5,
                    10,
                    20,
                    30,
                    60,
                    120,
                    300
                ]
            });
    }

    public void EmailEnqueued() => _enqueued.Add(1);

    public void DuplicateEmail() => _duplicates.Add(1);

    public void EmailSent() => _sent.Add(1);

    public void EmailsExpired(int count) => _expired.Add(count);

    public void EmailsAttemptsExhausted(int count) => _sendAttemptsExhausted.Add(count);

    public void EmailsSendingStale(int count) => _sendingStale.Add(count);

    public void EmailSendingErrorOccurred(EmailSenderFailureState failureState) =>
        _sendFailed.Add(1,
            new KeyValuePair<string, object?>(
                "failure_state",
                failureState.ToString()));

    public void EmailAckErrorOccurred() => _ackFailed.Add(1);

    public void RecordSendAttemptsToSuccess(int attempts) => _sendAttemptsToSuccess.Record(attempts);

    public void RecordMessageLatency(TimeSpan latency) => _messageLatency.Record(latency.TotalSeconds);
}
