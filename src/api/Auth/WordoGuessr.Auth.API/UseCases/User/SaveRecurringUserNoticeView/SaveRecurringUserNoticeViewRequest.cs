namespace WordoGuessr.Auth.API.UseCases.User.SaveRecurringUserNoticeView;

internal sealed record SaveRecurringUserNoticeViewRequest(string NoticeCode, bool DoNotShowAgain);
