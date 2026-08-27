using Microsoft.AspNetCore.Identity;

namespace WordoGuessr.Auth.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    // TODO: global rule + api
    public static readonly TimeSpan MinUserNameChangeInterval = TimeSpan.FromDays(30);

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public DateTimeOffset? UserNameChangedAt { get; private set; }

    private ApplicationUser()
    {
    }

    public ApplicationUser(Guid id, string email, string userName, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = id;
        Email = email;
        UserName = userName;
        EmailConfirmed = true;
        CreatedAt = createdAt;
    }

    public bool EnoughTimePassedFromLastUserNameChange(DateTimeOffset now)
    {
        if (UserNameChangedAt is null)
        {
            return true;
        }

        return now - UserNameChangedAt.Value > MinUserNameChangeInterval;
    }

    public void MarkLoggedIn(DateTimeOffset at)
    {
        LastLoginAt = at;
    }

    public void MarkUserNameChanged(DateTimeOffset at)
    {
        UserNameChangedAt = at;
    }
}
