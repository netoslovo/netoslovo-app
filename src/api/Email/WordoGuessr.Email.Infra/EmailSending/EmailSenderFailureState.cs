namespace WordoGuessr.Email.Infra.EmailSending;

public enum EmailSenderFailureState
{
    Retryable,
    Permanent,
    Unknown
}