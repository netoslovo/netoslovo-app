namespace WordoGuessr.API.BuildingBlocks.ModuleOrchestration;

public interface ICrossModuleTranscationalSession : IAsyncDisposable
{
    Task Commit(CancellationToken ct = default);
    TService GetRequiredService<TService>()
        where TService : notnull;
}
