using Wolverine.Attributes;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.App.Abstractions;
using WordoGuessr.Email.Contract.Commands;
using WordoGuessr.Email.Domain;

namespace WordoGuessr.Email.App.IntegrationHandlers;

public static class EnqueueEmailHandler
{
    [Transactional]
    public static Task Handle(
        EnqueueEmail command,
        IEmailQueue emailQueue,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        var createdAt = timeProvider.GetUtcNow();
        var message = new EmailMessage(
            EmailAddress.Create(command.To),
            EmailSubject.Create(command.Subject),
            EmailHtmlBody.Create(command.HtmlMessage),
            createdAt,
            command.SourceId,
            command.Ttl);

        return emailQueue.EnqueueMessage(message, ct);
    }
}
