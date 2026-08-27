using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.App.UseCases.Admin.PublishNewUserNameFilterVersion;

public sealed class PublishNewUserNameFilterVersionHandler
    : ICommandHandler<PublishNewUserNameFilterVersionCommand>
{
    private readonly IUserNameFilterVersionStore _userNameFilterVersionStore;

    public PublishNewUserNameFilterVersionHandler(
        IUserNameFilterVersionStore userNameFilterVersionStore)
    {
        _userNameFilterVersionStore = userNameFilterVersionStore
            ?? throw new ArgumentNullException(nameof(userNameFilterVersionStore));
    }

    public Task Handle(PublishNewUserNameFilterVersionCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return _userNameFilterVersionStore.PublishNewVersion(command.Version, ct);
    }
}
