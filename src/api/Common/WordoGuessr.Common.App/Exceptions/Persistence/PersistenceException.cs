namespace WordoGuessr.Common.App.Exceptions.Persistence;

public abstract class PersistenceException : Exception
{
    private const string DefaultMessage = "A persistence operation failed.";

    protected PersistenceException()
        : base(DefaultMessage)
    {
    }

    protected PersistenceException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    protected PersistenceException(string message)
        : base(message)
    {
    }

    protected PersistenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
