using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed partial class UserNameFilterUpdateWatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly UserNameFilterMetrics _metrics;

    public UserNameFilterUpdateWatcher(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<UserNameFilterUpdateWatcher> logger,
        UserNameFilterMetrics metrics)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(GetInterval(), _timeProvider);

        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var versionStore = scope.ServiceProvider.GetRequiredService<IUserNameFilterVersionStore>();
                var lifecyle = scope.ServiceProvider.GetRequiredService<IUserNameFilterLifecycle>();

                var storedVersion = await versionStore.GetLatestVersion(stoppingToken);
                _metrics.SetStoredVersion(storedVersion);

                var appVersion = lifecyle.GetCurrentVersion();
                _metrics.SetAppVersion(appVersion);

                if (appVersion == storedVersion)
                {
                    _logger.LogDebug("No update required. User name filter is already using version {Version}", storedVersion);
                    continue;
                }

                appVersion = await lifecyle.Update(storedVersion, stoppingToken);
                _metrics.SetAppVersion(appVersion);

                _logger.LogInformation("User name filter has been successfully updated to version {Version}", storedVersion);

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user name filter.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private TimeSpan GetInterval()
    {
        using var scope = _scopeFactory.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<UserNameFilterUpdateWatcherOptions>>();
        return options.Value.Interval;
    }
}
