namespace WordoGuessr.Common.App.Exceptions.Persistence;

public sealed class ForeignKeyConstraintViolationException : PersistenceException
{
    private const string DefaultMessage = "A foreign key constraint was violated.";

    public string? ConstraintName { get; }

    public ForeignKeyConstraintViolationException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public ForeignKeyConstraintViolationException(Exception innerException, string? constraintName)
        : base(DefaultMessage, innerException)
    {
        ConstraintName = constraintName;
    }

    public ForeignKeyConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ForeignKeyConstraintViolationException(string message, Exception innerException, string? constraintName)
        : base(message, innerException)
    {
        ConstraintName = constraintName;
    }
}
