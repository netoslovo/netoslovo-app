using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.SaveRecurringUserNoticeView;

internal sealed class SaveRecurringUserNoticeViewHandler
    : ICommandHandler<SaveRecurringUserNoticeViewCommand>
{
    private readonly IUserNoticeViewStore _store;

    public SaveRecurringUserNoticeViewHandler(IUserNoticeViewStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task Handle(
        SaveRecurringUserNoticeViewCommand command,
        CancellationToken ct)
    {
        var view = new RecurringUserNoticeView(
            command.UserId,
            command.Code,
            command.DoNotShowAgain);

        return _store.AddOrUpdateRecurringView(view, ct);
    }
}