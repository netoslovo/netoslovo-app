using Testcontainers.PostgreSql;
using Xunit;

namespace WordoGuessr.Testing.Common.PostgreSql;

public sealed class PostgreSqlAssemblyFixture : IAsyncLifetime
{
    private const string PostgreSqlVersion = "18.4";
    private PostgreSqlContainer? _postgreSqlContainer;
    private PostgreSqlFixtureConnectionPool? _connectionPool;
    private readonly TimeSpan _connectionWaitTimeout = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);
    private bool _initialized;
    private bool _disposed;

    public Task<PostgreSqlFixtureConnection> GetConnectionFromPool()
    {
        if (_connectionPool is null)
        {
            throw new InvalidOperationException($"{nameof(PostgreSqlAssemblyFixture)} pool has not been initialized");
        }
        return _connectionPool.GetConnection(_connectionWaitTimeout);
    }

    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            await _locker.WaitAsync();
            if (_initialized) return;

            _postgreSqlContainer = new PostgreSqlBuilder($"postgres:{PostgreSqlVersion}")
                .Build();
            await _postgreSqlContainer.StartAsync();

            _connectionPool = new PostgreSqlFixtureConnectionPool(_postgreSqlContainer.GetConnectionString());

            _initialized = true;
        }
        finally
        {
            _locker.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        try
        {
            await _locker.WaitAsync();
            if (_disposed) return;

            if (_postgreSqlContainer is not null)
            {
                await _postgreSqlContainer.DisposeAsync().ConfigureAwait(false);
            }

            _disposed = true;

        }
        finally
        {
            _locker.Release();
            _locker.Dispose();
        }
    }
}
