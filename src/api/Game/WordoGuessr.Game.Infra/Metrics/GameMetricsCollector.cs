using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WordoGuessr.Game.Infra.Metrics;

// TODO: distributed lease
internal sealed class GameMetricsCollector : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly GameInfraMetrics _metrics;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _activePlayerWindow;

    public GameMetricsCollector(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<GameMetricsCollector> logger,
        GameInfraMetrics metrics,
        IOptions<GameMetricsCollectorOptions> options)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));

        var value = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _interval = value.Interval;
        _activePlayerWindow = value.ActivePlayerWindow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval, _timeProvider);

        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
                var updatedSince = _timeProvider.GetUtcNow() - _activePlayerWindow;

                var activePlayers = await dbContext.SingleGames
                    .AsNoTracking()
                    .Where(game => game.UpdatedAt >= updatedSince)
                    .Select(game => game.PlayerId)
                    .Distinct()
                    .CountAsync(stoppingToken);

                _metrics.RecordActivePlayers(activePlayers);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Game metrics collection iteration failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
