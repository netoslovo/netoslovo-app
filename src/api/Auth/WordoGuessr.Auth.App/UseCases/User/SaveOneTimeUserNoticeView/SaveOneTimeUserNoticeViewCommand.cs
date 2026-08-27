using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Auth.App.UseCases.User.SaveOneTimeUserNoticeView;

public sealed record SaveOneTimeUserNoticeViewCommand(
    Guid UserId,
    string Code) : ICommand;