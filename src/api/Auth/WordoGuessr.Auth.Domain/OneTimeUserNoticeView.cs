namespace WordoGuessr.Auth.Domain;

public sealed class OneTimeUserNoticeView : UserNoticeView
{
    private OneTimeUserNoticeView()
    {
    }

    public OneTimeUserNoticeView(Guid userId, string noticeCode)
        : base(userId, noticeCode)
    {
    }
}