using StackExchange.Redis;

namespace WordoGuessr.WebAPI.Redis;

internal sealed record RedisConnections(
    Lazy<IConnectionMultiplexer> InMemory,
    Lazy<IConnectionMultiplexer> Persistent) : IDisposable
{
    public void Dispose()
    {
        if (InMemory.IsValueCreated)
        {
            InMemory.Value.Dispose();
        }

        if (Persistent.IsValueCreated)
        {
            Persistent.Value.Dispose();
        }
    }
}

internal static class RedisHepleres
{
    public static RedisConnections CreateRedisInfrastructure(
        IConfiguration config,
        string applicationName)
    {
        var inMemory = CreateRedisConnection(
            config,
            Const.InmemoryRedisConnectionStringName,
            applicationName);

        var persistent = CreateRedisConnection(
            config,
            Const.PersistentRedisConnectionStringName,
            applicationName);

        return new RedisConnections(inMemory, persistent);
    }

    private static Lazy<IConnectionMultiplexer> CreateRedisConnection(
        IConfiguration configuration,
        string connectionStringName,
        string applicationName)
    {
        return new Lazy<IConnectionMultiplexer>(() => GetConnection(configuration, connectionStringName, applicationName));
    }

    private static IConnectionMultiplexer GetConnection(
        IConfiguration configuration,
        string connectionStringName,
        string applicationName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");

        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;
        options.ClientName = applicationName;
        return ConnectionMultiplexer.Connect(options);
    }

}
