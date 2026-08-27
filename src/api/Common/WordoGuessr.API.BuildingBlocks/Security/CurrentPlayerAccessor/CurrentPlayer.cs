namespace WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;

using System.Diagnostics.CodeAnalysis;

public sealed class CurrentPlayer
{
    [MemberNotNullWhen(true, nameof(AuthenticatedUserId))]
    [MemberNotNullWhen(true, nameof(UserName))]
    [MemberNotNullWhen(true, nameof(Email))]
    [MemberNotNullWhen(true, nameof(Roles))]
    [MemberNotNullWhen(true, nameof(Permissions))]
    [MemberNotNullWhen(false, nameof(GuestId))]
    public bool IsAuthenticated { get; }

    public Guid? GuestId { get; }

    public Guid? AuthenticatedUserId { get; }

    public IReadOnlyCollection<string>? Roles { get; }
    public IReadOnlyCollection<string>? Permissions { get; }

    public Guid PlayerId =>
        AuthenticatedUserId
        ?? GuestId
        ?? throw new CurrentPlayerException("Current player id is missing.");

    public string? UserName { get; }

    public string? Email { get; }

    private CurrentPlayer(
        bool isAuthenticated,
        Guid? guestId,
        Guid? authenticatedUserId,
        string? userName,
        string? email,
        IReadOnlyCollection<string>? roles,
        IReadOnlyCollection<string>? permissions)
    {
        IsAuthenticated = isAuthenticated;
        GuestId = guestId;
        AuthenticatedUserId = authenticatedUserId;
        UserName = userName;
        Email = email;
        Roles = roles;
        Permissions = permissions;
    }

    public static CurrentPlayer Guest(Guid guestId)
    {
        return new CurrentPlayer(
            isAuthenticated: false,
            guestId: guestId,
            authenticatedUserId: null,
            userName: null,
            email: null,
            roles: null,
            permissions: null);
    }

    public static CurrentPlayer AuthenticatedUser(
        Guid authenticatedUserId,
        string? userName,
        string? email,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        return new CurrentPlayer(
            isAuthenticated: true,
            guestId: null,
            authenticatedUserId: authenticatedUserId,
            userName: userName,
            email: email,
            roles: roles,
            permissions: permissions);
    }
}
