namespace WordoGuessr.Common.Domain.ValueObjects;

public sealed class EmailSubject
{
    public string Value { get; }

    private EmailSubject(string value)
    {
        Value = value;
    }

    // TODO: validation
    public static EmailSubject Create(string value)
    {
        return new EmailSubject(value);
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailSubject other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}
