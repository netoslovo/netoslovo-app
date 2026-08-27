namespace WordoGuessr.Words.Contract;

public sealed class NotRequestedException : Exception
{
    public NotRequestedException(string propertyName)
        : base($"Words snapshot property '{propertyName}' was not requested.")
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}
