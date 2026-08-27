using Microsoft.Extensions.Logging;
using Xunit;

namespace WordoGuessr.Testing.Common.Logging;

internal sealed class XunitLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _categoryName;
    private readonly LogLevel _minimumLogLevel;

    public XunitLogger(
        ITestOutputHelper output,
        string categoryName,
        LogLevel minimumLogLevel)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _categoryName = categoryName ?? string.Empty;
        _minimumLogLevel = minimumLogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) =>
        _minimumLogLevel != LogLevel.None
        && logLevel != LogLevel.None
        && logLevel >= _minimumLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = $"{DateTimeOffset.UtcNow:HH:mm:ss zzz} [{logLevel}] {_categoryName}[{eventId.Id}] {formatter(state, exception)}";

        if (exception is not null)
            message += Environment.NewLine + exception;

        _output.WriteLine(message);
    }
}
