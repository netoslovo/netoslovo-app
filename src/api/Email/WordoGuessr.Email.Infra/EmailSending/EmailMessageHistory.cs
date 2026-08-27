using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Domain;

namespace WordoGuessr.Email.Infra.EmailSending;

internal sealed class EmailMessageHistory
{
    public Guid MessageId { get; private set; }

    public EmailAddress To { get; private set; } = null!;

    public EmailSubject Subject { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ExpireAt { get; private set; }

    public int Attempts { get; set; }

    public EmailMessageState State { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid SourceId { get; private set; }

    public DateTimeOffset ArchivedAt { get; private set; }

    public EmailMessageArchiveReason ArchiveReason { get; private set; }

    private EmailMessageHistory() { }
}

internal enum EmailMessageArchiveReason
{
    Sent,
    AttemptsExhausted,
    Expired,
    SendingStale,
    PermanentFailure
}
