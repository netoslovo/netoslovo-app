namespace WordoGuessr.Email.Infra.EmailSending.Exceptions;

public class EmailSenderSmtpException : EmailSenderException
{
    private const string EmailSenderSmtpExceptionMessage = "SMTP related error occurred while sending message";

    public EmailSenderSmtpException()
        : base(EmailSenderSmtpExceptionMessage, EmailSenderFailureState.Unknown)
    {
    }

    public EmailSenderSmtpException(Exception ex)
        : base(EmailSenderSmtpExceptionMessage, ex, EmailSenderFailureState.Unknown)
    {
    }

    public EmailSenderSmtpException(string message, Exception ex)
        : base(message, ex, EmailSenderFailureState.Unknown)
    {
    }

    public EmailSenderSmtpException(string message)
        : base(message, EmailSenderFailureState.Unknown)
    {
    }
}
