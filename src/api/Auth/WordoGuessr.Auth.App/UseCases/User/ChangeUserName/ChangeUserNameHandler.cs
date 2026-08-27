using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.ChangeUserName;

internal sealed class ChangeUserNameHandler
    : ICommandHandler<ChangeUserNameCommand, Result<ChangeUserNameError>>
{
    private readonly IUserService _userService;
    private readonly ISessionManager _sessionManager;
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ChangeUserNameHandler(
        IUserService userService,
        IAuthUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ISessionManager sessionManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public async Task<Result<ChangeUserNameError>> Handle(ChangeUserNameCommand command, CancellationToken ct = default)
    {
        using (var transactionScope = await _unitOfWork.BeginTransactionScope(ct))
        {
            var user = await _userService.FindById(command.PlayerId);
            if (user is null)
            {
                return Result<ChangeUserNameError>.Failure(ChangeUserNameError.UserNotFound);
            }

            var now = _timeProvider.GetUtcNow();
            var canChangeUserName = user.EnoughTimePassedFromLastUserNameChange(now);

            if (!canChangeUserName)
            {
                return Result<ChangeUserNameError>.Failure(ChangeUserNameError.TooFrequentAttempts);
            }

            var setUserNameResult = await _userService.SetUserName(user, command.NewUserName);
            if (!setUserNameResult.IsSuccess)
            {
                return Result<ChangeUserNameError>.Failure(MapError(setUserNameResult.Error));
            }

            user.MarkUserNameChanged(now);

            var updateResult = await _userService.Update(user);
            if (!updateResult.IsSuccess)
            {
                return Result<ChangeUserNameError>.Failure(MapError(updateResult.Error));
            }

            try
            {
                await transactionScope.Commit(ct);
            }
            catch (ConcurrencyConflictException)
            {
                return Result<ChangeUserNameError>.Failure(ChangeUserNameError.ConcurrencyFailure);
            }

            await _sessionManager.RefreshSignIn(command.PlayerId);
        }

        return Result<ChangeUserNameError>.Success();
    }

    private static ChangeUserNameError MapError(UserErrorCode errorCode)
    {
        return errorCode switch
        {
            UserErrorCode.UserNotFound => ChangeUserNameError.UserNotFound,
            UserErrorCode.InvalidUserName => ChangeUserNameError.InvalidUserName,
            UserErrorCode.UnsafeUserName => ChangeUserNameError.UnsafeUserName,
            UserErrorCode.DuplicateUserName => ChangeUserNameError.DuplicateUserName,
            UserErrorCode.TooFrequentChangeUserNameAttempts => ChangeUserNameError.TooFrequentAttempts,
            UserErrorCode.ConcurrencyFailure => ChangeUserNameError.ConcurrencyFailure,
            _ => throw new InvalidOperationException($"Unexpected user error code: {errorCode}.")
        };
    }
}
