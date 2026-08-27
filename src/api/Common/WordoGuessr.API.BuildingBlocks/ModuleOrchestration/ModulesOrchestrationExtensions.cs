using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public static class ModulesOrchestrationExtensions
{
    public static IServiceCollection AddModulesOrchestration(this IServiceCollection sc, string appConnectionString)
    {
        sc.AddScoped<DbConnection>(sp => new NpgsqlConnection(appConnectionString));
        sc.AddScoped<OrchestratedTransactionAccessor>();
        sc.AddScoped<IModulesOrchestrator, ModulesOrchestrator>();

        return sc;
    }

    public static IServiceCollection AddOrchestratableDbContext<TContext>(
        this IServiceCollection sc,
        Func<IServiceProvider, DbConnection, TContext> orchestratedContextFactory,
        Func<IServiceProvider, TContext> simpleContextFactory)
            where TContext : DbContext
    {
        sc.AddScoped(sp =>
        {
            var transaction = sp.GetService<OrchestratedTransactionAccessor>()?.CurrentTransaction;
            var crossModuleTranscationalModeIsActive = transaction is not null;

            TContext dbContext;
            if (crossModuleTranscationalModeIsActive)
            {
                var connection = sp.GetRequiredService<DbConnection>()
                    ?? throw new InvalidOperationException(
                        "Orchestratable DbContext couldn't be added without adding modules orchestration components");

                dbContext = orchestratedContextFactory(sp, connection);
                dbContext.Database.UseTransaction(transaction);
            }
            else
            {
                dbContext = simpleContextFactory(sp);
            }

            return dbContext;
        });

        return sc;
    }
}
