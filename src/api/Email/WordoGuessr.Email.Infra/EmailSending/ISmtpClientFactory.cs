using MailKit.Net.Smtp;

namespace WordoGuessr.Email.Infra.EmailSending;

public interface ISmtpClientFactory
{
    ISmtpClient Create();
}
