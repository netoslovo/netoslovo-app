using Microsoft.AspNetCore.Identity;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class UserValidator : IUserValidator<ApplicationUser>
{
    public const string UnsafeUserName = nameof(UnsafeUserName);
    private readonly IUserNameFilter _userNameFilter;
    private readonly IdentityErrorDescriber _errorDescriber;

    public UserValidator(
        IUserNameFilter userNameFilter,
        IdentityErrorDescriber errorDescriber)
    {
        _userNameFilter = userNameFilter ?? throw new ArgumentNullException(nameof(userNameFilter));
        _errorDescriber = errorDescriber ?? throw new ArgumentNullException(nameof(errorDescriber));
    }

    public async Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager,
        ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);

        var errors = new List<IdentityError>();

        var userNameErrors = await ValidateUserName(manager, user);
        errors.AddRange(userNameErrors);

        if (manager.Options.User.RequireUniqueEmail)
        {
            var emailErrors = await ValidateEmail(manager, user);
            errors.AddRange(emailErrors);
        }

        if (errors.Count > 0)
        {
            return IdentityResult.Failed(errors.ToArray());
        }

        return IdentityResult.Success;
    }

    private async Task<IdentityError[]> ValidateUserName(
        UserManager<ApplicationUser> manager,
        ApplicationUser user)
    {
        var userName = user.UserName;

        if (userName is null || !UserName.IsValid(userName))
        {
            return [_errorDescriber.InvalidUserName(userName)];
        }

        var isSafe = await _userNameFilter.IsSafe(userName);
        if (!isSafe)
        {
            return [new IdentityError
            {
                Code = UnsafeUserName
            }];
        }

        var owner = await manager.FindByNameAsync(userName);

        if (owner is not null && owner.Id != user.Id)
        {
            return [_errorDescriber.DuplicateUserName(userName)];
        }

        return [];
    }

    private async Task<IdentityError[]> ValidateEmail(
        UserManager<ApplicationUser> manager,
        ApplicationUser user)
    {
        var email = user.Email;

        if (email is null || !EmailAddress.IsValid(email))
        {
            return [_errorDescriber.InvalidEmail(email)];
        }

        var owner = await manager.FindByEmailAsync(email);

        if (owner is not null && owner.Id != user.Id)
        {
            return [_errorDescriber.DuplicateEmail(email)];
        }

        return [];
    }
}
