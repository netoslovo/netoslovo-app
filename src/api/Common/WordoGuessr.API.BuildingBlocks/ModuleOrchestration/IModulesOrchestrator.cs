namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public interface IModulesOrchestrator
{
    Task<CrossModuleTranscationalSession> Begin(CancellationToken ct);
}
