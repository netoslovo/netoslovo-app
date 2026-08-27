using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.Sagas.Infra.Storage;

public sealed class SagasDbContextOptions : IDbContextOptions
{
    public static string Name => "Sagas:DbContext";

    public bool EnableDebugLogging { get; set; }
}
