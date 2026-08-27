using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.App.Services;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;

internal sealed class VerifyEmailOtpHandler : ICommandHandler<VerifyEmailOtpCommand, Result<VerifyEmailOtpError>>
{
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly IOtpCodeHasher _otpCodeHasher;
    private readonly TimeProvider _timeProvider;
    private readonly AuthOtpOptions _otpOptions;
    private readonly ISessionManager _sessionManager;
    private readonly IGuestSessionManager _guestSessionManager;
    private readonly ICurrentPlayerAccessor _currentPlayerAccessor;
    private readonly AuthAppMetrics _appMetrics;

    public VerifyEmailOtpHandler(
        IAuthUnitOfWork unitOfWork,
        IOtpCodeHasher otpCodeHasher,
        TimeProvider timeProvider,
        IOptions<AuthOtpOptions> otpOptions,
        ISessionManager sessionManager,
        IGuestSessionManager guestSessionManager,
        ICurrentPlayerAccessor currentPlayerAccessor,
        AuthAppMetrics appMetrics)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _otpCodeHasher = otpCodeHasher ?? throw new ArgumentNullException(nameof(otpCodeHasher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _otpOptions = otpOptions?.Value ?? throw new ArgumentNullException(nameof(otpOptions));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _guestSessionManager = guestSessionManager ?? throw new ArgumentNullException(nameof(guestSessionManager));
        _currentPlayerAccessor = currentPlayerAccessor ?? throw new ArgumentNullException(nameof(currentPlayerAccessor));
        _appMetrics = appMetrics ?? throw new ArgumentNullException(nameof(appMetrics));
    }

    public async Task<Result<VerifyEmailOtpError>> Handle(VerifyEmailOtpCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            return await HandleCore(command, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.UnhandledError);
            throw;
        }
    }

    private async Task<Result<VerifyEmailOtpError>> HandleCore(
        VerifyEmailOtpCommand command,
        CancellationToken ct)
    {
        var currentPlayer = _currentPlayerAccessor.GetCurrentPlayer();
        if (currentPlayer.IsAuthenticated)
        {
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.AlreadyAuthenticated);
            return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.AlreadyAuthenticated);
        }

        var currentTime = _timeProvider.GetUtcNow();

        OtpChallenge? challenge;
        using (var transactionScope = await _unitOfWork.BeginTransactionScope(ct))
        {
            challenge = await _unitOfWork.OtpChallenges.FindById(command.ChallengeId, ct);
            if (challenge is null)
            {
                _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.NotFound);
                _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
                return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.OtpNotFound);
            }

            if (currentPlayer.GuestId.Value != challenge.RequestedByGuestId)
            {
                _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.GuestSessionMismatch);
                _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
                return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidGuestSession);
            }

            challenge.RegisterAttempt();

            try
            {
                await transactionScope.Commit(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _appMetrics.OtpLoginFailed(OtpLoginFailureReason.ConcurrencyFailure);
                return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
            }
        }

        if (challenge.IsConsumed)
        {
            _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.AlreadyConsumed);
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
            return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
        }

        if (challenge.IsExpired(currentTime))
        {
            _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.Expired);
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
            return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
        }

        if (challenge.AttemptCount > _otpOptions.MaxAttempts)
        {
            _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.AttemptsExhausted);
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
            return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
        }

        if (!_otpCodeHasher.Verify(command.Code, challenge.CodeHash))
        {
            _appMetrics.OtpVerificationFailed(OtpVerificationFailureReason.InvalidCode);
            _appMetrics.OtpLoginFailed(OtpLoginFailureReason.Otp);
            return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
        }

        ApplicationUser? user;
        var newUserCreated = false;
        using (var transactionScope = await _unitOfWork.BeginTransactionScope(ct))
        {
            challenge.MarkConsumed(currentTime);

            user = await _unitOfWork.Users.FindByEmail(challenge.Email);
            if (user is null)
            {
                var createUserResult = await _unitOfWork.Users.CreateConfirmedEmailUser(
                    challenge.RequestedByGuestId,
                    challenge.Email);

                if (!createUserResult.IsSuccess)
                {
                    var failureReason = createUserResult.Error switch
                    {
                        UserErrorCode.DuplicateEmail => OtpLoginFailureReason.EmailAlreadyRegistered,
                        UserErrorCode.ConcurrencyFailure => OtpLoginFailureReason.ConcurrencyFailure,
                        _ => OtpLoginFailureReason.UserCreationFailure
                    };
                    _appMetrics.OtpLoginFailed(failureReason);

                    var error = createUserResult.Error switch
                    {
                        UserErrorCode.DuplicateEmail => VerifyEmailOtpError.EmailAlreadyRegistered,
                        UserErrorCode.ConcurrencyFailure => VerifyEmailOtpError.ConcurrencyFailure,
                        _ => VerifyEmailOtpError.GetOrAddUserError
                    };
                    return Result<VerifyEmailOtpError>.Failure(error);
                }

                user = createUserResult.Value;
                newUserCreated = true;
            }

            user.MarkLoggedIn(currentTime);

            var updateUserResult = await _unitOfWork.Users.Update(user);
            if (!updateUserResult.IsSuccess)
            {
                _appMetrics.OtpLoginFailed(OtpLoginFailureReason.UserUpdateFailure);
                return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.GetOrAddUserError);
            }

            try
            {
                await transactionScope.Commit(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _appMetrics.OtpLoginFailed(OtpLoginFailureReason.ConcurrencyFailure);
                return Result<VerifyEmailOtpError>.Failure(VerifyEmailOtpError.InvalidOtpState);
            }
        }

        if (newUserCreated)
        {
            _appMetrics.NewUserRegistered();
        }

        await _sessionManager.SignIn(user.Id);
        _guestSessionManager.ClearGuestSession();

        _appMetrics.OtpLoginSucceeded();
        return Result<VerifyEmailOtpError>.Success();
    }
}
