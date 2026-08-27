using System.Diagnostics.Metrics;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.Auth.App;

internal sealed class AuthAppMetrics
{
    private readonly Counter<int> _otpRequested;
    private readonly Counter<int> _otpRequestRateLimit;
    private readonly Counter<int> _otpVerificationFailed;
    private readonly Counter<int> _usersRegistered;
    private readonly Counter<int> _successfulOtpLogins;
    private readonly Counter<int> _otpLoginFailures;

    public AuthAppMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.CreatePrefixedMeter("Auth.App");
        _otpRequested = meter.CreateCounter<int>("auth.otp.requested");
        _otpRequestRateLimit = meter.CreateCounter<int>("auth.otp.request.rate_limit");
        _otpVerificationFailed = meter.CreateCounter<int>("auth.otp.verification.failed");
        _usersRegistered = meter.CreateCounter<int>("auth.users.registered");
        _successfulOtpLogins = meter.CreateCounter<int>("auth.otp.login.success");
        _otpLoginFailures = meter.CreateCounter<int>("auth.otp.login.errors");
    }

    public void OtpRequested() => _otpRequested.Add(1);

    public void OtpRequestRateLimit() => _otpRequestRateLimit.Add(1);

    public void OtpVerificationFailed(OtpVerificationFailureReason reason) =>
        _otpVerificationFailed.Add(1, new KeyValuePair<string, object?>("reason", reason.ToString()));

    public void NewUserRegistered() => _usersRegistered.Add(1);

    public void OtpLoginSucceeded() => _successfulOtpLogins.Add(1);

    public void OtpLoginFailed(OtpLoginFailureReason reason) =>
        _otpLoginFailures.Add(1, new KeyValuePair<string, object?>("reason", reason.ToString()));
}

internal enum OtpLoginFailureReason
{
    Otp,
    AlreadyAuthenticated,
    EmailAlreadyRegistered,
    UserCreationFailure,
    UserUpdateFailure,
    ConcurrencyFailure,
    UnhandledError
}

internal enum OtpVerificationFailureReason
{
    NotFound,
    GuestSessionMismatch,
    AlreadyConsumed,
    Expired,
    AttemptsExhausted,
    InvalidCode
}
