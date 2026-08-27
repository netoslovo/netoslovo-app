namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class UniqueConstraintViolationException : PersistenceException
{
    private const string DefaultMessage = "A unique constraint was violated.";

    public string? ConstraintName { get; }

    public UniqueConstraintViolationException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public UniqueConstraintViolationException(Exception innerException, string? constraintName)
        : base(DefaultMessage, innerException)
    {
        ConstraintName = constraintName;
    }

    public UniqueConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public UniqueConstraintViolationException(string message, Exception innerException, string? constraintName)
        : base(message, innerException)
    {
        ConstraintName = constraintName;
    }
}
