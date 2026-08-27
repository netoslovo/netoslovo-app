using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public sealed class ModulesOrchestrator : IModulesOrchestrator
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ModulesOrchestrator(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<CrossModuleTranscationalSession> Begin(CancellationToken ct)
    {
        var scope = _serviceScopeFactory.CreateAsyncScope();

        try
        {
            var connection = scope.ServiceProvider.GetRequiredService<DbConnection>();
            await connection.OpenAsync(ct);

            var transaction = await connection.BeginTransactionAsync(ct);
            var transactionAccessor = scope.ServiceProvider
                .GetRequiredService<OrchestratedTransactionAccessor>();

            transactionAccessor.CurrentTransaction = transaction;

            return new CrossModuleTranscationalSession(scope, transaction);
        }
        catch
        {
            await scope.DisposeAsync();
            throw;
        }
    }
}
