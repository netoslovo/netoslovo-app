using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.ShouldShowUserNotice;

internal sealed record ShouldShowUserNoticeHandler
    : IQueryHandler<ShouldShowUserNoticeQuery, bool>
{
    private readonly IUserNoticeViewStore _userNoticeViewStore;

    public ShouldShowUserNoticeHandler(IUserNoticeViewStore userNoticeViewStore)
    {
        _userNoticeViewStore = userNoticeViewStore ?? throw new ArgumentNullException(nameof(userNoticeViewStore));
    }

    public async Task<bool> Handle(ShouldShowUserNoticeQuery query, CancellationToken ct)
    {
        var savedNoticeView = await _userNoticeViewStore.FindUserNoticeView(query.UserId, query.NoticeCode, ct);

        return savedNoticeView switch
        {
            null => true,
            RecurringUserNoticeView recurring when !recurring.DoNotShowAgain => true,
            _ => false
        };
    }
}
