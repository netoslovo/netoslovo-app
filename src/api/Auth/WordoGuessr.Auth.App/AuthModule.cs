using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Contract;

namespace WordoGuessr.Auth.App;

public sealed class AuthModule : IAuthModule
{
    private readonly IUserService _userService;

    public AuthModule(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public Task<IReadOnlyDictionary<Guid, string>> GetUserNames(
        IReadOnlyList<Guid> userIds,
        CancellationToken ct = default)
    {
        return _userService.GetUserNames(userIds);
    }
}