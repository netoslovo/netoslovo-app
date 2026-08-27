
using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Auth.App.UseCases.User.SaveRecurringUserNoticeView;

public sealed record SaveRecurringUserNoticeViewCommand(
    Guid UserId,
    string Code,
    bool DoNotShowAgain) : ICommand;