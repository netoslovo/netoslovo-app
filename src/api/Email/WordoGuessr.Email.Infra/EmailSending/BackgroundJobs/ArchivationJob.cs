using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WordoGuessr.Email.Infra.EmailSending.BackgroundJobs;

internal sealed class ArchivationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ArchivationJob> _logger;
    private readonly ArchivationJobOptions _options;

    public ArchivationJob(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<ArchivationJob> logger,
        IOptions<ArchivationJobOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval, _timeProvider);
        do
        {
            await TryArchiveMessages(stoppingToken);
            await TryClearExpiredDeduplicationEntries(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task TryArchiveMessages(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueueInternal>();
            await emailQueue.ArchiveMessagesInTerminationStates(_options.BatchSize, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email queue archivation iteration failed");
        }
    }

    private async Task TryClearExpiredDeduplicationEntries(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueueInternal>();
            await emailQueue.ClearExpiredDeduplicationEntries(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email deduplication cleanup iteration failed");
        }
    }
}
