using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.App.Abstractions;

public interface IOtpRateLimiter
{
    Task<bool> TryAcquire(EmailAddress address, DateTimeOffset at, CancellationToken ct = default);
}