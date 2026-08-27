using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.API.BuildingBlocks.DbContextCommon;

public interface IDbContextOptions : INamedOptions
{
    bool EnableDebugLogging { get; }
}