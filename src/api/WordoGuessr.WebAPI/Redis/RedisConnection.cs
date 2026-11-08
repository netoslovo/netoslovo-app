using StackExchange.Redis;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.WebAPI.Redis;

internal sealed class RedisConnection : IDisposable
{
    private readonly Lazy<IConnectionMultiplexer> _connection;

    public IConnectionMultiplexer Value => _connection.Value;

    public RedisConnection(
        IConfiguration configuration,
        string applicationName)
    {
        _connection = new(() => Connect(configuration, applicationName));
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }

    private static IConnectionMultiplexer Connect(
        IConfiguration configuration,
        string applicationName)
    {
        var connectionString = configuration.GetRequiredConnectionString(
            Const.RedisConnectionStringName);

        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;
        options.ClientName = applicationName;

        return ConnectionMultiplexer.Connect(options);
    }
}
