using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class UsersMetricsCollector : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private readonly AuthInfraMetrics _metrics;
    private readonly TimeSpan _interval;

    public UsersMetricsCollector(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<UsersMetricsCollector> logger,
        AuthInfraMetrics metrics,
        IOptions<UsersMetricsCollectorOptions> options)
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
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var totalUsers = await userManager.Users.CountAsync(stoppingToken);

                _metrics.RecordTotalUsers(totalUsers);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Users metrics collection iteration failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
