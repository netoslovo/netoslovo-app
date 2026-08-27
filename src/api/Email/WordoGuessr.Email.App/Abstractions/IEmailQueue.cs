using WordoGuessr.Email.Domain;

namespace WordoGuessr.Email.App.Abstractions;

public interface IEmailQueue
{
    Task<int> EnqueueMessage(EmailMessage message, CancellationToken ct = default);
}
