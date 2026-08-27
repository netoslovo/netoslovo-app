#if DEBUG

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class DesignTimeGameDbContextFactory
    : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=wordoguessr;Username=postgres;Password=postgres");
        return new AuthDbContext(
            Options.Create(new AuthDbContextOptions { }),
            NullLoggerFactory.Instance,
            connection);
    }
}
#endif
