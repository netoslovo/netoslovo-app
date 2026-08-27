using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Auth.App.UseCases.User.ShouldShowUserNotice;

public sealed record ShouldShowUserNoticeQuery(Guid UserId, string NoticeCode)
    : IQuery<bool>;
