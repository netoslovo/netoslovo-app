using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using PGDeployer;
using WordoGuessr.Testing.Common.Logging;
using WordoGuessr.Testing.Common.PostgreSql;
using Xunit;

namespace WordoGuessr.Testing.Common;

public abstract class DbContextFixture<TDbContext> : IAsyncLifetime
    where TDbContext : DbContext
{
    private readonly PostgreSqlAssemblyFixture _postgreSqlFixture;
    private readonly string _journalTableName;
    private PostgreSqlFixtureConnection? _connection;

    protected DbContextFixture(
        PostgreSqlAssemblyFixture postgreSqlFixture,
        string journalTableName)
    {
        _postgreSqlFixture = postgreSqlFixture ?? throw new ArgumentNullException(nameof(postgreSqlFixture));
        _journalTableName = journalTableName ?? throw new ArgumentNullException(nameof(journalTableName));
    }

    public async ValueTask InitializeAsync()
    {
        _connection = await _postgreSqlFixture.GetConnectionFromPool();

        var schemaMigrations = new AssemblyScriptsSource(typeof(TDbContext).Assembly);
        var deployerConfig = new DeployerConfiguration(_connection.ConnectionString, true, schemaMigrations)
        {
            JournalTableName = new PostgresqlJournalTable("public", _journalTableName)
        };
        var deployer = new Deployer(deployerConfig);
        deployer.Deploy();
    }

    protected TDbContext CreateDbContext(
        Func<ILoggerFactory, DbConnection, TDbContext> createDbContext) =>
        CreateDbContext(NullLoggerFactory.Instance, createDbContext);

    protected TDbContext CreateDbContext(
        ILoggerFactory loggerFactory,
        Func<ILoggerFactory, DbConnection, TDbContext> createDbContext)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(createDbContext);

        if (_connection is null)
            throw new InvalidOperationException($"{GetType().Name} is not initialized");

        return createDbContext(loggerFactory, new NpgsqlConnection(_connection.ConnectionString));
    }

    protected TDbContext CreateDbContext(
        ITestOutputHelper output,
        Func<ILoggerFactory, DbConnection, TDbContext> createDbContext)
    {
        ArgumentNullException.ThrowIfNull(output);

        return CreateDbContext(
            new XunitLoggerFactory(output, LogLevel.Debug),
            createDbContext);
    }

    public ValueTask DisposeAsync()
    {
        _connection?.Dispose();
        return ValueTask.CompletedTask;
    }
}
