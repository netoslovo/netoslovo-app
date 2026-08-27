using Microsoft.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Infra.Database;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class EfUserNameFilterVersionStore : IUserNameFilterVersionStore
{
    private readonly AuthDbContext _authDbContext;
    private readonly TimeProvider _timeProvider;

    public EfUserNameFilterVersionStore(AuthDbContext authDbContext, TimeProvider timeProvider)
    {
        _authDbContext = authDbContext ?? throw new ArgumentNullException(nameof(authDbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<int> GetLatestVersion(CancellationToken ct = default)
    {
        var activeVersion = await _authDbContext.Set<UserNameFilterVersion>().SingleAsync(v => v.IsActive, ct);
        return activeVersion.Version;
    }

    public async Task PublishNewVersion(int version, CancellationToken ct = default)
    {
        var currentActiveVersion = await _authDbContext.Set<UserNameFilterVersion>()
            .SingleAsync(v => v.IsActive, ct);

        if (currentActiveVersion.Version == version) return;
        currentActiveVersion.Deactivate();

        var userNameFilterVersion = new UserNameFilterVersion(version, _timeProvider.GetUtcNow(), isActive: true);
        _authDbContext.Set<UserNameFilterVersion>().Add(userNameFilterVersion);
        await _authDbContext.SaveChangesAsync(ct);
    }
}