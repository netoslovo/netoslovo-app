using System.Net;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.App.Services;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Contract.Commands;

namespace WordoGuessr.Auth.App.UseCases.User.RequestEmailOtp;

internal sealed class RequestEmailOtpHandler
    : ICommandHandler<RequestEmailOtpCommand, Result<Guid, RequestEmailOtpError>>
{
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly ICurrentPlayerAccessor _currentPlayerAccessor;
    private readonly IOtpCodeGenerator _otpCodeGenerator;
    private readonly IOtpCodeHasher _otpCodeHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IOtpRateLimiter _otpRateLimiter;
    private readonly AuthOtpOptions _otpOptions;
    private readonly AuthAppMetrics _appMetrics;

    public RequestEmailOtpHandler(
        IAuthUnitOfWork unitOfWork,
        ICurrentPlayerAccessor currentPlayerAccessor,
        IOtpCodeGenerator otpCodeGenerator,
        IOtpCodeHasher otpCodeHasher,
        TimeProvider timeProvider,
        IOptions<AuthOtpOptions> otpOptions,
        IOtpRateLimiter otpRateLimiter,
        AuthAppMetrics appMetrics)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentPlayerAccessor = currentPlayerAccessor ?? throw new ArgumentNullException(nameof(currentPlayerAccessor));
        _otpCodeGenerator = otpCodeGenerator ?? throw new ArgumentNullException(nameof(otpCodeGenerator));
        _otpCodeHasher = otpCodeHasher ?? throw new ArgumentNullException(nameof(otpCodeHasher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _otpRateLimiter = otpRateLimiter ?? throw new ArgumentNullException(nameof(otpRateLimiter));
        _otpOptions = otpOptions?.Value ?? throw new ArgumentNullException(nameof(otpOptions));
        _appMetrics = appMetrics ?? throw new ArgumentNullException(nameof(appMetrics));
    }

    public async Task<Result<Guid, RequestEmailOtpError>> Handle(
        RequestEmailOtpCommand command,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var email = EmailAddress.Create(command.Email.Trim().ToLowerInvariant());

        var currentPlayer = _currentPlayerAccessor.GetCurrentPlayer();
        if (currentPlayer.IsAuthenticated)
        {
            return Result<Guid, RequestEmailOtpError>.Failure(RequestEmailOtpError.AlreadyAuthenticated);
        }

        var now = _timeProvider.GetUtcNow();
        var rateLimitAcquired = await _otpRateLimiter.TryAcquire(email, now, ct);
        if (!rateLimitAcquired)
        {
            _appMetrics.OtpRequestRateLimit();
            return Result<Guid, RequestEmailOtpError>.Failure(RequestEmailOtpError.RateLimitError);
        }

        var otpCode = _otpCodeGenerator.Generate(_otpOptions.CodeLength);
        var otpHash = _otpCodeHasher.Hash(otpCode);

        var challenge = new OtpChallenge(
            email,
            otpHash,
            currentPlayer.GuestId.Value,
            now,
            _otpOptions.TimeToLive);

        using (var transactionScope = await _unitOfWork.BeginTransactionScope(ct))
        {
            _unitOfWork.OtpChallenges.Add(challenge);

            var enqueueEmailCommand = new EnqueueEmail(
                email.Value,
                _otpOptions.Subject,
                CreateOtpEmailBody(otpCode),
                challenge.Id,
                _otpOptions.TimeToLive);

            await _unitOfWork.Outbox.Send(enqueueEmailCommand);
            await transactionScope.Commit(ct);
        }

        _appMetrics.OtpRequested();
        return Result<Guid, RequestEmailOtpError>.Success(challenge.Id);
    }

    private static string CreateOtpEmailBody(string otpCode)
    {
        var encodedCode = WebUtility.HtmlEncode(otpCode);

        return $"""
            <p>Ваш разовый код для входа:</p>
            <p><strong>{encodedCode}</strong></p>
            <p>Если вы не запрашивали код, можете проигнорировать это сообщение.</p>
            """;
    }
}
