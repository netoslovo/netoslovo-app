using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.Game.Infra;

public sealed class GameDbContextOptions : IDbContextOptions
{
    public static string Name => "Game:DbContext";

    public bool EnableDebugLogging { get; set; }
}
