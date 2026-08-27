using System.Data.Common;

namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public sealed class OrchestratedTransactionAccessor
{
    public DbTransaction? CurrentTransaction { get; set; }
}
