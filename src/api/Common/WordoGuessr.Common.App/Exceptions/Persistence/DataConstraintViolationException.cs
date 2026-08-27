namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class DataConstraintViolationException : PersistenceException
{
    private const string DefaultMessage = "A data constraint was violated.";

    public string? ConstraintName { get; }

    public DataConstraintViolationException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public DataConstraintViolationException(Exception innerException, string? constraintName)
        : base(DefaultMessage, innerException)
    {
        ConstraintName = constraintName;
    }

    public DataConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DataConstraintViolationException(string message, Exception innerException, string? constraintName)
        : base(message, innerException)
    {
        ConstraintName = constraintName;
    }
}
