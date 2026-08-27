using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.Auth.Infra.Database;

public sealed class AuthDbContextOptions : IDbContextOptions, INamedOptions
{
    public static string Name => "Auth:DbContext";

    public bool EnableDebugLogging { get; set; }
}
