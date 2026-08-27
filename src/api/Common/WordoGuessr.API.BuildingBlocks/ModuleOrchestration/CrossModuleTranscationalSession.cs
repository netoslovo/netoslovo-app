using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public sealed class CrossModuleTranscationalSession : ICrossModuleTranscationalSession
{
    private readonly AsyncServiceScope _scope;
    private readonly DbTransaction _transaction;
    private bool _committed;

    public CrossModuleTranscationalSession(AsyncServiceScope scope, DbTransaction transaction)
    {
        _scope = scope;
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public TService GetRequiredService<TService>() where TService : notnull
        => _scope.ServiceProvider.GetRequiredService<TService>();

    public async Task Commit(CancellationToken ct = default)
    {
        await _transaction.CommitAsync(ct);
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
