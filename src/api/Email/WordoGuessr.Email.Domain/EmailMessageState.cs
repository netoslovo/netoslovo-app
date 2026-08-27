namespace WordoGuessr.Email.Domain;

public enum EmailMessageState
{
    Pending,
    Sending,
    Sent,
    FailedRetryable,
    FailedPermanent
}

public static class EmailMessageStateExtensions
{
    public static bool IsAcknowledgeable(this EmailMessageState state) =>
        state == EmailMessageState.Sent ||
        state == EmailMessageState.FailedRetryable ||
        state == EmailMessageState.FailedPermanent;
}
