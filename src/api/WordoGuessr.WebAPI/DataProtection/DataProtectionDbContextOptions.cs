using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.WebAPI.DataProtection;

internal sealed class DataProtectionDbContextOptions : IDbContextOptions
{
    public static string Name => "DataProtection:DbContext";

    public bool EnableDebugLogging { get; set; }
}
