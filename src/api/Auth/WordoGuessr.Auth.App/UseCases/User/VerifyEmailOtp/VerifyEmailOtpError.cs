namespace WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;

public enum VerifyEmailOtpError
{
    OtpNotFound,
    InvalidOtpState,
    AlreadyAuthenticated,
    InvalidGuestSession,
    EmailAlreadyRegistered,
    ConcurrencyFailure,
    GetOrAddUserError
}
