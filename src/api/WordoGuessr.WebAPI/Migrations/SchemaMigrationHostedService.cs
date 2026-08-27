using Microsoft.Extensions.Options;
using PGDeployer;

namespace WordoGuessr.WebAPI.Migrations;

internal sealed class SchemaMigrationHostedService : IHostedService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Deployer _deployer;
    private readonly bool _standaloneMigrationMode;
    private readonly IHostApplicationLifetime _appLifetime;

    public SchemaMigrationHostedService(
        ILoggerFactory loggerFactory,
        IOptions<SchemaMigratorOptions> options,
        IHostApplicationLifetime appLifetime)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));

        var deployerConfig = new DeployerConfiguration(options.Value.ConnectionString, true, options.Value.ScriptsSources)
        {
            JournalTableName = new PostgresqlJournalTable(options.Value.JournalSchema, options.Value.JournalTable),
            UpgrageLogger = new MSUpgradeLogger(_loggerFactory.CreateLogger<Deployer>())
        };
        _deployer = new Deployer(deployerConfig);

        _standaloneMigrationMode = options.Value.StandaloneMigrationModeEnabled;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _deployer.Deploy();

        if (_standaloneMigrationMode)
        {
            _appLifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
