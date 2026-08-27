using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.SaveOneTimeUserNoticeView;

internal sealed class SaveOneTimeUserNoticeViewHandler
    : ICommandHandler<SaveOneTimeUserNoticeViewCommand>
{
    private readonly IUserNoticeViewStore _store;

    public SaveOneTimeUserNoticeViewHandler(IUserNoticeViewStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task Handle(
        SaveOneTimeUserNoticeViewCommand command,
        CancellationToken ct)
    {
        var view = new OneTimeUserNoticeView(
            command.UserId,
            command.Code);

        return _store.AddOneTimeView(view, ct);
    }
}