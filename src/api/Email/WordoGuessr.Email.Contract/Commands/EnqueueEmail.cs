namespace WordoGuessr.Email.Contract.Commands;

public sealed record EnqueueEmail(
    string To,
    string Subject,
    string HtmlMessage,
    Guid SourceId,
    TimeSpan? Ttl = default);