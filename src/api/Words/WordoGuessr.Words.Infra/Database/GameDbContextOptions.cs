using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.Words.Infra.Database;

public sealed class WordsDbContextOptions : IDbContextOptions
{
    public static string Name => "Words:DbContext";

    public bool EnableDebugLogging { get; set; }
}
