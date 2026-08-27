namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilterVersion
{
    public int Id { get; }
    public int Version { get; }
    public DateTimeOffset PublishedAt { get; }
    public bool IsActive { get; private set; }

    public UserNameFilterVersion(
        int version,
        DateTimeOffset publishedAt,
        bool isActive)
    {
        Version = version;
        PublishedAt = publishedAt;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}