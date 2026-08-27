namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

public sealed class BlockedUserNameWord
{
    public string Word { get; } = null!;
    public DateTimeOffset CreatedAt { get; }
}