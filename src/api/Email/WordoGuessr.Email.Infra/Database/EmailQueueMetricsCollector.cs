using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Email.Infra.EmailSending;

namespace WordoGuessr.Email.Infra.Database;

internal sealed class EmailQueueMetricsCollector : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly EmailQueueMetrics _metrics;
    private readonly TimeSpan _interval;

    public EmailQueueMetricsCollector(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<EmailQueueMetricsCollector> logger,
        EmailQueueMetrics metrics,
        IOptions<EmailQueueMetricsCollectorOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _interval = options?.Value.Interval ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval, _timeProvider);

        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueueInternal>();

                var readyMetrics = await emailQueue.GetReadyMessagesMetrics(stoppingToken);
                _metrics.RecordReadyMessagesCount(readyMetrics.TotalReady);

                var oldestReadyAge = readyMetrics.OldestReady.HasValue
                    ? _timeProvider.GetUtcNow() - readyMetrics.OldestReady.Value
                    : TimeSpan.Zero;
                _metrics.RecordOldestReadyAge(oldestReadyAge);

                var totalMessagesCount = await emailQueue.GetTotalMessagesCount(stoppingToken);
                _metrics.RecordTotalMessagesCount(totalMessagesCount);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email queue metrics collection iteration failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
