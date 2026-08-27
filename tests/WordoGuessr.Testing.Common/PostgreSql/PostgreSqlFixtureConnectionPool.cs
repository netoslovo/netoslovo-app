using System.Collections.Concurrent;
using Npgsql;

namespace WordoGuessr.Testing.Common.PostgreSql;

internal class PostgreSqlFixtureConnectionPool
{
    private const int PoolSize = 16;
    private const int PoolRetryDelayMs = 100;

    private readonly ConcurrentQueue<PostgreSqlFixtureConnection> _pool
        = new ConcurrentQueue<PostgreSqlFixtureConnection>();

    private readonly string _systemConnectionString;

    internal PostgreSqlFixtureConnectionPool(string systemConnectionString, int poolSize = PoolSize)
    {
        _systemConnectionString = systemConnectionString;

        for (int i = 0; i < poolSize; i++)
        {
            var currentDbId = Guid.NewGuid().ToString();
            using (var dbCon = new NpgsqlConnection(systemConnectionString))
            using (var command = new NpgsqlCommand(BuildDbCreateCommand(currentDbId), dbCon))
            {
                dbCon.Open();
                command.ExecuteNonQuery();
            }

            var connectionString = BuildConnectionString(systemConnectionString, currentDbId);
            var connection = new PostgreSqlFixtureConnection(connectionString, this);
            _pool.Enqueue(connection);
        }
    }

    internal async Task<PostgreSqlFixtureConnection> GetConnection(TimeSpan waitTimeout)
    {
        using var cts = new CancellationTokenSource();
        try
        {
            cts.CancelAfter(waitTimeout);
            return await GetConnectionInternal(cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new PostgreSqlFixtureException("Timeout fired while waiting for connection from the pool");
        }
    }

    internal void Release(PostgreSqlFixtureConnection postgreSqlFixtureConnection)
    {
        var conBuilder = new NpgsqlConnectionStringBuilder(postgreSqlFixtureConnection.ConnectionString);
        var dbId = conBuilder.Database;

        if (string.IsNullOrWhiteSpace(dbId))
        {
            throw new PostgreSqlFixtureException(
                "Couldn't extract DataBase from connection string. Check if connection string is valid");
        }

        using (var dbCon = new NpgsqlConnection(_systemConnectionString))
        using (var dropCommand = new NpgsqlCommand(BuildDbDropCommand(dbId), dbCon))
        using (var createCommand = new NpgsqlCommand(BuildDbCreateCommand(dbId), dbCon))
        {
            dbCon.Open();
            dropCommand.ExecuteNonQuery();
            createCommand.ExecuteNonQuery();
        }

        NpgsqlConnection.ClearPool(new NpgsqlConnection(postgreSqlFixtureConnection.ConnectionString));
        _pool.Enqueue(new PostgreSqlFixtureConnection(postgreSqlFixtureConnection.ConnectionString, this));
    }

    private static string BuildDbCreateCommand(string dbId) =>
        $"""
        CREATE DATABASE "{dbId}"; 
        """;

    private static string BuildDbDropCommand(string dbId) =>
        $"""
        DROP DATABASE "{dbId}" WITH (FORCE);
        """;

    private static string BuildConnectionString(string conStr, string dbId)
    {
        var builder = new NpgsqlConnectionStringBuilder(conStr)
        {
            Database = dbId
        };
        return builder.ConnectionString;
    }

    private async Task<PostgreSqlFixtureConnection> GetConnectionInternal(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            if (_pool.TryDequeue(out var connection))
            {
                return connection;
            }
            await Task.Delay(PoolRetryDelayMs, ct);
        }
    }
}
