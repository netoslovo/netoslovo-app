namespace WordoGuessr.Testing.Common.PostgreSql;

public sealed class PostgreSqlFixtureConnection : IDisposable
{
    public string ConnectionString { get; }

    private readonly PostgreSqlFixtureConnectionPool _connectionPool;
    private readonly Lock _lock = new Lock();
    private bool _disposed;

    internal PostgreSqlFixtureConnection(string connectionString, PostgreSqlFixtureConnectionPool connectionPool)
    {
        ConnectionString = connectionString;
        _connectionPool = connectionPool;
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            if (_disposed) return;
            _connectionPool.Release(this);

            _disposed = true;
        }
    }
}
