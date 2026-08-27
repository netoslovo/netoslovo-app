using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.App.UseCases.User.Logout;

internal sealed class LogoutHandler : ICommandHandler<LogoutCommand>
{
    private readonly ISessionManager _sessionManager;

    public LogoutHandler(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public async Task Handle(LogoutCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _sessionManager.SignOut();
    }
}
