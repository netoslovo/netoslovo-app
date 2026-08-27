using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Email.Infra.EmailSending;

public interface IEmailSender
{
    Task NoOp(CancellationToken ct = default);

    Task SendEmail(
        EmailAddress to,
        EmailSubject subject,
        EmailHtmlBody htmlMessage,
        Guid messageId,
        CancellationToken ct = default);
}
