using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Game.Infra;
using WordoGuessr.Testing.Common;
using WordoGuessr.Testing.Common.PostgreSql;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class GameDbContextFixture : DbContextFixture<GameDbContext>
{
    public GameDbContextFixture(PostgreSqlAssemblyFixture postgreSqlFixture)
        : base(
            postgreSqlFixture,
            "game_schema_migrations_journal")
    {
    }

    public GameDbContext CreateDbContext() =>
        CreateDbContext(CreateGameDbContext);

    public GameDbContext CreateDbContext(ITestOutputHelper output) =>
        CreateDbContext(output, CreateGameDbContext);

    private static GameDbContext CreateGameDbContext(ILoggerFactory loggerFactory, DbConnection connection) =>
        new GameDbContext(
            Options.Create(new GameDbContextOptions { EnableDebugLogging = true }),
            loggerFactory,
            connection);
}
