namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class TransientPersistenceException : PersistenceException, ITransientException
{
    private const string DefaultMessage = "A transient persistence error occurred.";

    public TransientPersistenceException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public TransientPersistenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
