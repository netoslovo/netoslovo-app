using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.Abstractions;

public interface IOtpChallengeRepository
{
    Task<OtpChallenge?> FindById(Guid id, CancellationToken ct = default);

    void Add(OtpChallenge challenge);
}
