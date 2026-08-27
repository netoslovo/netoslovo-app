namespace WordoGuessr.Auth.App.Dto;

public sealed class AuthMeDto
{
    public bool IsAuthenticated { get; }

    public Guid Id { get; }

    public string? UserName { get; }

    public string? Email { get; }

    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Permissions { get; }

    private AuthMeDto(
        bool isAuthenticated,
        Guid id,
        string? userName,
        string? email,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        IsAuthenticated = isAuthenticated;
        Id = id;
        UserName = userName;
        Email = email;
        Roles = roles;
        Permissions = permissions;
    }

    public static AuthMeDto Guest(Guid guestId)
    {
        return new AuthMeDto(
            isAuthenticated: false,
            id: guestId,
            userName: null,
            email: null,
            roles: [],
            permissions: []);
    }

    public static AuthMeDto AuthenticatedUser(
        Guid authenticatedUserId,
        string? userName,
        string? email,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        return new AuthMeDto(
            isAuthenticated: true,
            id: authenticatedUserId,
            userName: userName,
            email: email,
            roles: roles,
            permissions: permissions);
    }
}