using Xunit;

namespace WordoGuessr.Testing.Common.PostgreSql;

public sealed class PostgreSqlClassFixture : IAsyncLifetime
{
    public string ConnectionString =>
        _connection?.ConnectionString
        ?? throw new InvalidOperationException("PostgreSQL fixture is not initialized.");

    private readonly PostgreSqlAssemblyFixture _assemblyFixture;
    private PostgreSqlFixtureConnection? _connection;

    private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);
    private bool _initialized;
    private bool _disposed;

    public PostgreSqlClassFixture(PostgreSqlAssemblyFixture assemblyFixture)
    {
        _assemblyFixture = assemblyFixture ?? throw new ArgumentNullException(nameof(assemblyFixture));
    }

    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;
        await _locker.WaitAsync();
        try
        {
            if (_initialized) return;

            _connection = await _assemblyFixture.GetConnectionFromPool();
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
        await _locker.WaitAsync();
        try
        {
            if (_disposed) return;

            _connection?.Dispose();

            _disposed = true;
        }
        finally
        {
            _locker.Release();
            _locker.Dispose();
        }
    }
}
