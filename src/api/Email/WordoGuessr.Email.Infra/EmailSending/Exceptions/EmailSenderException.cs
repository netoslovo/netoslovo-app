namespace WordoGuessr.Email.Infra.EmailSending.Exceptions;

public class EmailSenderException : Exception
{
    private const string DefaultMessage = "Unable to send email message";

    public EmailSenderFailureState FailureState { get; }

    public EmailSenderException(EmailSenderFailureState failureState)
        : base(DefaultMessage)
    {
        FailureState = failureState;
    }

    public EmailSenderException(Exception ex, EmailSenderFailureState failureState)
        : base(DefaultMessage, ex)
    {
        FailureState = failureState;
    }

    public EmailSenderException(string message, Exception ex, EmailSenderFailureState failureState)
        : base(message, ex)
    {
        FailureState = failureState;
    }

    public EmailSenderException(string message, EmailSenderFailureState failureState)
        : base(message)
    {
        FailureState = failureState;
    }
}
