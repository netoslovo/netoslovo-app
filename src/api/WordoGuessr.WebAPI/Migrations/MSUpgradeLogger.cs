using PGDeployer;

namespace WordoGuessr.WebAPI.Migrations;

internal sealed class MSUpgradeLogger : UpgrageLogger
{
    private readonly ILogger _logger;

    public MSUpgradeLogger(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void WriteError(string format, object[] args)
    {
        _logger.LogError(format, args);
    }

    protected override void WriteInformation(string format, object[] args)
    {
        _logger.LogInformation(format, args);
    }

    protected override void WriteWarning(string format, object[] args)
    {
        _logger.LogWarning(format, args);
    }
}
