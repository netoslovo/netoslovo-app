using Microsoft.Extensions.Logging;
using Xunit;

namespace WordoGuessr.Testing.Common.Logging;

public sealed class XunitLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _output;
    private readonly LogLevel _minimumLogLevel;

    public XunitLoggerFactory(
        ITestOutputHelper output,
        LogLevel minimumLogLevel = LogLevel.Information)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _minimumLogLevel = minimumLogLevel;
    }

    public ILogger CreateLogger(string categoryName) =>
        new XunitLogger(_output, categoryName, _minimumLogLevel);

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }
}
