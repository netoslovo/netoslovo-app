namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class ConcurrencyConflictException : PersistenceException, IConcurrencyException
{
    private const string DefaultMessage = "A persistence concurrency conflict occurred.";

    public ConcurrencyConflictException(string message)
        : base(message)
    {

    }

    public ConcurrencyConflictException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
