using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Email.Domain;
using WordoGuessr.Email.Infra.EmailSending.Exceptions;

namespace WordoGuessr.Email.Infra.EmailSending.BackgroundJobs;

internal sealed class SendingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SendingJob> _logger;
    private readonly EmailMetrics _metrics;
    private readonly SendingJobOptions _options;

    public SendingJob(
        IServiceScopeFactory scopeFactory,
        IEmailSender emailSender,
        TimeProvider timeProvider,
        ILogger<SendingJob> logger,
        EmailMetrics metrics,
        IOptions<SendingJobOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval, _timeProvider);
        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueueInternal>();
                await Handle(emailQueue, _options.BatchSize, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending iteration failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task Handle(
        IEmailQueueInternal emailQueue,
        int batchSize,
        CancellationToken ct)
    {
        var nextBatch = await emailQueue.GetNextBatch(batchSize, ct);

        if (nextBatch.Count == 0)
        {
            _logger.LogDebug("No new email messages found; sending no-op command");
            await _emailSender.NoOp(ct);
            _logger.LogDebug("No-op command sent successfully");
            return;
        }

        List<Exception> acknowledgeExceptions = [];
        foreach (var message in nextBatch)
        {
            var result = await SendMessage(message, ct);
            try
            {
                await emailQueue.Acknowledge(
                    result.MessageId,
                    result.MessageVersion,
                    result.State,
                    result.At,
                    ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                acknowledgeExceptions.Add(ex);
                _metrics.EmailAckErrorOccurred();
            }
        }

        if (acknowledgeExceptions.Count > 0)
        {
            throw new AggregateException(
                "One or more email messages failed to process.",
                acknowledgeExceptions);
        }
    }

    private async Task<SendResult> SendMessage(EmailMessage message, CancellationToken ct)
    {
        try
        {
            await _emailSender.SendEmail(
                message.To,
                message.Subject,
                message.Body,
                message.SourceId,
                ct);

            _metrics.EmailSent();
            _metrics.RecordSendAttemptsToSuccess(message.Attempts);
            _metrics.RecordMessageLatency(_timeProvider.GetUtcNow() - message.CreatedAt);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        // Unknown FailureState считаем Permanent
        catch (EmailSenderException ex) when (ex.FailureState == EmailSenderFailureState.Retryable)
        {
            _logger.LogError(ex, "Retryable error occurred while sending email {EmailMessageId}", message.Id);
            _metrics.EmailSendingErrorOccurred(EmailSenderFailureState.Retryable);

            return new SendResult(
                message.Id,
                message.Version,
                EmailMessageState.FailedRetryable,
                _timeProvider.GetUtcNow());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email {EmailMessageId}", message.Id);
            _metrics.EmailSendingErrorOccurred(EmailSenderFailureState.Permanent);

            return new SendResult(
                message.Id,
                message.Version,
                EmailMessageState.FailedPermanent,
                _timeProvider.GetUtcNow());
        }

        return new SendResult(
            message.Id,
            message.Version,
            EmailMessageState.Sent,
            _timeProvider.GetUtcNow());
    }

    private sealed record SendResult(
        Guid MessageId,
        uint MessageVersion,
        EmailMessageState State,
        DateTimeOffset At);
}
