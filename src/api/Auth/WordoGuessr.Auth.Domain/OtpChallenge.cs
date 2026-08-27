using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.Domain;

public sealed class OtpChallenge
{
    public Guid Id { get; private set; }

    public EmailAddress Email { get; private set; } = null!;

    public string CodeHash { get; private set; } = null!;

    public Guid RequestedByGuestId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public TimeSpan Ttl { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public int AttemptCount { get; private set; }

    public OtpChallenge(
        EmailAddress email,
        string codeHash,
        Guid requestedByGuestId,
        DateTimeOffset createdAt,
        TimeSpan ttl)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "OTP time-to-live must be positive.");
        }

        Id = Guid.CreateVersion7(createdAt);
        Email = email;
        CodeHash = codeHash;
        RequestedByGuestId = requestedByGuestId;
        CreatedAt = createdAt;
        Ttl = ttl;
    }

    private OtpChallenge() { }

    public bool IsExpired(DateTimeOffset now) => now >= CreatedAt.Add(Ttl);

    public bool IsConsumed => ConsumedAt.HasValue;

    public void RegisterAttempt()
    {
        AttemptCount++;
    }

    public void MarkConsumed(DateTimeOffset consumedAt)
    {
        ConsumedAt = consumedAt;
    }
}
