using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.Email.Infra.Database;

public sealed class EmailDbContextOptions : IDbContextOptions, INamedOptions
{
    public static string Name => "Email:DbContext";

    public bool EnableDebugLogging { get; set; }
}
