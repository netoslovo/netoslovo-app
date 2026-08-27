namespace WordoGuessr.Email.Infra.EmailSending.Exceptions;

public sealed class EmailSenderTimeoutException : EmailSenderException
{
    private const string EmailSenderTimeoutExceptionMessage = "Timeout trggiered while sending message";

    public EmailSenderTimeoutException(EmailSenderFailureState failureState)
        : base(EmailSenderTimeoutExceptionMessage, failureState)
    {
    }

    public EmailSenderTimeoutException(Exception ex, EmailSenderFailureState failureState)
        : base(EmailSenderTimeoutExceptionMessage, ex, failureState)
    {
    }

    public EmailSenderTimeoutException(
        string message,
        Exception ex,
        EmailSenderFailureState failureState) : base(message, ex, failureState)
    {
    }

    public EmailSenderTimeoutException(string message, EmailSenderFailureState failureState)
        : base(message, failureState)
    {
    }
}
