namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class UnknownPersistenceException : PersistenceException
{
    public UnknownPersistenceException(Exception innerException)
        : base("An unknown persistence operation error occurred.", innerException)
    {
    }
}
