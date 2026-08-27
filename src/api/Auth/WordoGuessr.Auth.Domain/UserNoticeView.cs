namespace WordoGuessr.Auth.Domain;

public abstract class UserNoticeView
{
    public Guid UserId { get; private set; }
    public string NoticeCode { get; private set; } = null!;

    protected UserNoticeView()
    {
    }

    protected UserNoticeView(Guid userId, string noticeCode)
    {
        UserId = userId;
        NoticeCode = noticeCode;
    }
}