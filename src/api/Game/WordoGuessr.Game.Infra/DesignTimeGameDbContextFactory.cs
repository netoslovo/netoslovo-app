#if DEBUG

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace WordoGuessr.Game.Infra;

internal sealed class DesignTimeGameDbContextFactory
    : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(string[] args)
    {
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=wordoguessr;Username=postgres;Password=postgres");
        return new GameDbContext(
            Options.Create(new GameDbContextOptions { }),
            NullLoggerFactory.Instance,
            connection);
    }
}
#endif
