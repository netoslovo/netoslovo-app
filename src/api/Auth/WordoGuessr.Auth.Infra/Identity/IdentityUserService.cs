using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.App.Services;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.Infra.Identity;

// TODO: errors logging
internal sealed class IdentityUserService : IUserService
{
    private const string UserNameIndex = "ux_users_normalized_user_name";
    private const string EmailIndex = "ux_users_normalized_email";
    private const string UserRolePrimaryKey = "pk_auth_user_roles";
    private const int MaxUserNameCreateAttempts = 5;
    private const int MaxUserNameShortPoolGenerationAttempts = 2;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TimeProvider _timeProvider;
    private readonly IUserNameGenerator _userNameGenerator;

    public IdentityUserService(
        UserManager<ApplicationUser> userManager,
        TimeProvider timeProvider,
        IUserNameGenerator userNameGenerator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _userNameGenerator = userNameGenerator ?? throw new ArgumentNullException(nameof(userNameGenerator));
    }

    public async Task<ApplicationUser?> FindByEmail(EmailAddress email)
    {
        ArgumentNullException.ThrowIfNull(email);

        try
        {
            var user = await _userManager.FindByEmailAsync(email.Value);
            return user;
        }
        catch (Exception ex)
        {
            throw PersistenceExceptionsMapper.MapToPersistenceException(ex);
        }
    }

    public async Task<ApplicationUser?> FindById(Guid id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return user;
        }
        catch (Exception ex)
        {
            throw PersistenceExceptionsMapper.MapToPersistenceException(ex);
        }
    }

    public async Task<Result<ApplicationUser, UserErrorCode>> CreateConfirmedEmailUser(
        Guid id,
        EmailAddress email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var createdAt = _timeProvider.GetUtcNow();
        ApplicationUser? user = null;

        for (int i = 1; i <= MaxUserNameCreateAttempts; i++)
        {
            var userName = i <= MaxUserNameShortPoolGenerationAttempts
                ? _userNameGenerator.Generate()
                : _userNameGenerator.GenerateFromExtendedPool();

            if (user is null)
            {
                user = new ApplicationUser(
                    id,
                    email.Value,
                    userName,
                    createdAt);
            }
            else
            {
                user.UserName = userName;
            }

            IdentityResult result;
            try
            {
                result = await _userManager.CreateAsync(user);
            }
            catch (Exception ex)
            {
                var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
                var error = MapDatabaseUserError(mappedException);

                if (error.HasValue)
                {
                    if (error.Value == UserErrorCode.DuplicateUserName) continue;

                    return Result<ApplicationUser, UserErrorCode>.Failure(error.Value);
                }

                throw mappedException;
            }

            if (result.Succeeded)
            {
                return Result<ApplicationUser, UserErrorCode>.Success(user);
            }

            var errorCode = MapIdentityErrors(result);
            switch (errorCode)
            {
                case UserErrorCode.DuplicateUserName:
                case UserErrorCode.UnsafeUserName:
                case UserErrorCode.InvalidUserName:
                    continue;

                default:
                    return Result<ApplicationUser, UserErrorCode>.Failure(errorCode);
            }
        }

        throw new UserServiceException(
            new UserServiceError(
                UserErrorCode.UserNameGenerationAttemptsExhausted.ToString(),
                $"Failed to generate an available user name after {MaxUserNameCreateAttempts} attempts."));
    }

    public async Task<Result<UserErrorCode>> Update(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        IdentityResult result;
        try
        {
            result = await _userManager.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
            return MapDatabaseResult(mappedException);
        }

        if (!result.Succeeded)
        {
            return Result<UserErrorCode>.Failure(MapIdentityErrors(result));
        }

        return Result<UserErrorCode>.Success();
    }

    public async Task<IReadOnlyCollection<string>> GetUserRoles(Guid userId)
    {
        ApplicationUser? user;
        try
        {
            user = await _userManager.FindByIdAsync(userId.ToString());
        }
        catch (Exception ex)
        {
            throw PersistenceExceptionsMapper.MapToPersistenceException(ex);
        }

        if (user is null)
        {
            return [];
        }

        IList<string> roles;
        try
        {
            roles = await _userManager.GetRolesAsync(user);
        }
        catch (Exception ex)
        {
            throw PersistenceExceptionsMapper.MapToPersistenceException(ex);
        }

        return roles.ToArray();
    }

    public async Task<Result<UserErrorCode>> AssignRoleToUser(
        Guid userId,
        string role)
    {
        ApplicationUser? user;
        try
        {

            user = await _userManager.FindByIdAsync(userId.ToString());
        }

        catch (Exception ex)
        {
            var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
            return MapDatabaseResult(mappedException);
        }

        if (user is null)
        {
            return Result<UserErrorCode>.Failure(UserErrorCode.UserNotFound);
        }

        try
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return Result<UserErrorCode>.Success();
            }
        }
        catch (Exception ex)
        {
            var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
            return MapDatabaseResult(mappedException);
        }

        IdentityResult result;
        try
        {
            result = await _userManager.AddToRoleAsync(user, role);
        }
        catch (Exception ex)
        {
            var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
            if (mappedException is UniqueConstraintViolationException { ConstraintName: UserRolePrimaryKey })
            {
                return Result<UserErrorCode>.Success();
            }

            return MapDatabaseResult(mappedException);
        }

        bool isFailure = !result.Succeeded
            && result.Errors.All(e => e.Code != nameof(IdentityErrorDescriber.UserAlreadyInRole));

        if (isFailure)
        {
            var error = MapIdentityErrors(result);
            return Result<UserErrorCode>.Failure(error);
        }
        return Result<UserErrorCode>.Success();
    }

    public async Task<Result<UserErrorCode>> SetUserName(
        ApplicationUser user,
        string userName)
    {
        ArgumentNullException.ThrowIfNull(user);

        IdentityResult result;
        try
        {
            result = await _userManager.SetUserNameAsync(user, userName);

        }
        catch (Exception ex)
        {
            var mappedException = PersistenceExceptionsMapper.MapToPersistenceException(ex);
            var error = MapDatabaseUserError(mappedException);

            if (error.HasValue)
            {
                return Result<UserErrorCode>.Failure(error.Value);
            }

            throw mappedException;
        }

        if (!result.Succeeded)
        {
            return Result<UserErrorCode>.Failure(MapIdentityErrors(result));
        }

        return Result<UserErrorCode>.Success();
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetUserNames(IReadOnlyList<Guid> userIds)
    {
        try
        {
            var result = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName!);

            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            throw PersistenceExceptionsMapper.MapToPersistenceException(ex);
        }
    }

    private static UserErrorCode MapIdentityErrors(IdentityResult result)
    {
        if (result.Succeeded)
        {
            throw new InvalidOperationException("Cannot map successful IdentityResult to user error.");
        }

        var errors = result.Errors.ToArray();

        if (errors.Length == 0)
        {
            throw new UserServiceException([]);
        }

        var mappedErrors = errors
            .Select(error => new
            {
                IdentityError = error,
                IsMapped = TryMapIdentityError(error.Code, out var userErrorCode),
                UserErrorCode = userErrorCode
            })
            .ToArray();

        if (mappedErrors.Any(error => !error.IsMapped))
        {
            throw new UserServiceException(errors
                .Select(error => new UserServiceError(error.Code, error.Description))
                .ToArray());
        }

        var priority = new[]
        {
            UserErrorCode.DuplicateEmail,
            UserErrorCode.InvalidEmail,
            UserErrorCode.DuplicateUserName,
            UserErrorCode.InvalidUserName,
            UserErrorCode.UnsafeUserName,
            UserErrorCode.ConcurrencyFailure
        };

        return priority.First(code => mappedErrors.Any(error => error.UserErrorCode == code));
    }

    private static bool TryMapIdentityError(string identityErrorCode, out UserErrorCode userErrorCode)
    {
        userErrorCode = identityErrorCode switch
        {
            UserValidator.UnsafeUserName => UserErrorCode.UnsafeUserName,
            nameof(IdentityErrorDescriber.InvalidUserName) => UserErrorCode.InvalidUserName,
            nameof(IdentityErrorDescriber.InvalidEmail) => UserErrorCode.InvalidEmail,
            nameof(IdentityErrorDescriber.DuplicateUserName) => UserErrorCode.DuplicateUserName,
            nameof(IdentityErrorDescriber.DuplicateEmail) => UserErrorCode.DuplicateEmail,
            nameof(IdentityErrorDescriber.ConcurrencyFailure) => UserErrorCode.ConcurrencyFailure,
            _ => default
        };

        return userErrorCode != default;
    }

    private static Result<UserErrorCode> MapDatabaseResult(PersistenceException exception)
    {
        var error = MapDatabaseUserError(exception);

        if (error.HasValue)
        {
            return Result<UserErrorCode>.Failure(error.Value);
        }

        throw exception;
    }

    private static UserErrorCode? MapDatabaseUserError(PersistenceException exception)
        => exception switch
        {
            ConcurrencyConflictException => UserErrorCode.ConcurrencyFailure,
            UniqueConstraintViolationException { ConstraintName: UserNameIndex } => UserErrorCode.DuplicateUserName,
            UniqueConstraintViolationException { ConstraintName: EmailIndex } => UserErrorCode.DuplicateEmail,
            _ => null
        };
}
