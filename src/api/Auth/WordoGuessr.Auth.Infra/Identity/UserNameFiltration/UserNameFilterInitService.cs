using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilterInitService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UserNameFilterInitService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var versionStore = scope.ServiceProvider.GetRequiredService<IUserNameFilterVersionStore>();
        var lifecyle = scope.ServiceProvider.GetRequiredService<IUserNameFilterLifecycle>();
        var version = await versionStore.GetLatestVersion(stoppingToken);
        await lifecyle.Init(version, stoppingToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
