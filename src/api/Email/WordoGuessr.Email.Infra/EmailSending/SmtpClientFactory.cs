using MailKit.Net.Smtp;

namespace WordoGuessr.Email.Infra.EmailSending;

internal sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create() =>
        new SmtpClient();
}
