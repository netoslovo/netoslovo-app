using Microsoft.Extensions.DependencyInjection;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilter :
    IUserNameFilter,
    IUserNameFilterLifecycle
{
    private UserNameFilterContext? _context;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SemaphoreSlim _lifetimeLock = new SemaphoreSlim(1, 1);

    public UserNameFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task Init(int version, CancellationToken ct)
    {
        if (_context is not null) return;

        await _lifetimeLock.WaitAsync(ct);
        try
        {
            if (_context is not null) return;
            _context = await LoadContext(version, ct);
        }
        finally
        {
            _lifetimeLock.Release();
        }
    }

    public async Task<int> Update(int version, CancellationToken ct = default)
    {
        await _lifetimeLock.WaitAsync(ct);
        try
        {
            if (_context is null) throw new InvalidOperationException(
                $"{nameof(UserNameFilter)} should be initialized first using Init() method before using");

            if (_context.Version == version) return _context.Version;

            _context = await LoadContext(version, ct);
            return _context.Version;
        }
        finally
        {
            _lifetimeLock.Release();
        }
    }

    public Task<bool> IsSafe(string userName)
    {
        var context = _context
            ?? throw new InvalidOperationException(
                $"{nameof(UserNameFilter)} should be initialized first using Init() method before using");

        userName = userName.ToLowerInvariant();
        var isSafe = context.IsSafe(userName);
        return Task.FromResult(isSafe);
    }

    public int GetCurrentVersion() => _context?.Version ?? -1;

    private async Task<UserNameFilterContext> LoadContext(int version, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var blockList = scope.ServiceProvider.GetRequiredService<IUserNameBlockList>();
        var replacementList = scope.ServiceProvider.GetRequiredService<IUserNameReplacementList>();

        var context = new UserNameFilterContext(version);
        var words = await blockList.GetBlockedWords(version, ct);
        foreach (var word in words)
        {
            context.AddWord(word);
        }

        var replacements = await replacementList.GetReplacements(version, ct);
        context.LoadReplacements(replacements);
        return context;
    }
}
