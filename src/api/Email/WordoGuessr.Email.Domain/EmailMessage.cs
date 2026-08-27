using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Email.Domain;

public sealed class EmailMessage
{
    public Guid Id { get; }

    public EmailAddress To { get; } = null!;

    public EmailSubject Subject { get; } = null!;

    public EmailHtmlBody Body { get; } = null!;

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ExpireAt { get; }

    public int Attempts { get; set; }

    public EmailMessageState State { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public uint Version { get; }

    public Guid SourceId { get; }

    public EmailMessage(
        EmailAddress to,
        EmailSubject subject,
        EmailHtmlBody body,
        DateTimeOffset createdAt,
        Guid sourceId,
        TimeSpan? ttl = default)
    {
        Id = Guid.CreateVersion7(createdAt);
        To = to;
        Subject = subject;
        Body = body;
        CreatedAt = createdAt;
        State = EmailMessageState.Pending;
        UpdatedAt = createdAt;
        SourceId = sourceId;

        if (ttl is not null)
        {
            ExpireAt = CreatedAt + ttl.Value;
        }
    }

    private EmailMessage() { }
}
