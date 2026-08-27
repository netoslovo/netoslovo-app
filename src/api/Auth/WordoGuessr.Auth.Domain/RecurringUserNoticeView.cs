namespace WordoGuessr.Auth.Domain;

public sealed class RecurringUserNoticeView : UserNoticeView
{
    public bool DoNotShowAgain { get; private set; }

    private RecurringUserNoticeView()
    {
    }

    public RecurringUserNoticeView(
        Guid userId,
        string noticeCode,
        bool doNotShowAgain)
        : base(userId, noticeCode)
    {
        DoNotShowAgain = doNotShowAgain;
    }

    public void Disable()
    {
        if (DoNotShowAgain) return;

        DoNotShowAgain = true;
    }
}