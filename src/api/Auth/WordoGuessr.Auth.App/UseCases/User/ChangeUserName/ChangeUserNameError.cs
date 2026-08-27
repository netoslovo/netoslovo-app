namespace WordoGuessr.Auth.App.UseCases.User.ChangeUserName;

public enum ChangeUserNameError
{
    UserNotFound,
    InvalidUserName,
    UnsafeUserName,
    DuplicateUserName,
    TooFrequentAttempts,
    ConcurrencyFailure,
    Unauthorized
}
