using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.App.Abstractions;

public interface IUserService
{
    Task<ApplicationUser?> FindByEmail(EmailAddress email);

    Task<ApplicationUser?> FindById(Guid id);

    Task<Result<ApplicationUser, UserErrorCode>> CreateConfirmedEmailUser(
        Guid id,
        EmailAddress email);

    Task<Result<UserErrorCode>> Update(ApplicationUser user);

    Task<IReadOnlyCollection<string>> GetUserRoles(Guid userId);

    Task<Result<UserErrorCode>> AssignRoleToUser(Guid userId, string role);

    Task<Result<UserErrorCode>> SetUserName(ApplicationUser user, string userName);

    Task<IReadOnlyDictionary<Guid, string>> GetUserNames(IReadOnlyList<Guid> userIds);
}
