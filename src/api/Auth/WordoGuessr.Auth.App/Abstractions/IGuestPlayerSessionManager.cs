namespace WordoGuessr.Auth.App.Abstractions;

public interface IGuestSessionManager
{
    Guid GetOrCreateGuestPlayerSession();

    void ClearGuestSession();
}
