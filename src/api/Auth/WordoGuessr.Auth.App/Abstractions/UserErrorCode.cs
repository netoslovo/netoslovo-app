namespace WordoGuessr.Auth.App.Abstractions;

public enum UserErrorCode
{
    UserNotFound,
    InvalidUserName,
    UnsafeUserName,
    InvalidEmail,
    DuplicateUserName,
    DuplicateEmail,
    TooFrequentChangeUserNameAttempts,
    ConcurrencyFailure,
    UserNameGenerationAttemptsExhausted
}
