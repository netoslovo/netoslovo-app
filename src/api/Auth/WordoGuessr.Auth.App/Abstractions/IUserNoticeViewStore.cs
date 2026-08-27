using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.Abstractions;

public interface IUserNoticeViewStore
{
    Task<UserNoticeView?> FindUserNoticeView(Guid userId, string code, CancellationToken ct = default);

    Task AddOneTimeView(OneTimeUserNoticeView view, CancellationToken ct = default);

    Task AddOrUpdateRecurringView(RecurringUserNoticeView view, CancellationToken ct = default);
}
