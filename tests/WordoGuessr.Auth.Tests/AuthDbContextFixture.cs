using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.Auth.Infra.Database;
using WordoGuessr.Testing.Common;
using WordoGuessr.Testing.Common.PostgreSql;
using Xunit;

namespace WordoGuessr.Auth.Tests;

public sealed class AuthDbContextFixture : DbContextFixture<AuthDbContext>
{
    public AuthDbContextFixture(PostgreSqlAssemblyFixture postgreSqlFixture)
        : base(
            postgreSqlFixture,
            "auth_schema_migrations_journal")
    {
    }

    public AuthDbContext CreateDbContext() =>
        CreateDbContext(CreateAuthDbContext);

    public AuthDbContext CreateDbContext(ITestOutputHelper output) =>
        CreateDbContext(output, CreateAuthDbContext);

    private static AuthDbContext CreateAuthDbContext(ILoggerFactory loggerFactory, DbConnection connection) =>
        new AuthDbContext(
            Options.Create(new AuthDbContextOptions { EnableDebugLogging = true }),
            loggerFactory,
            connection);
}
