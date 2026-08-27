namespace WordoGuessr.Common.Domain.ValueObjects;

public sealed class EmailHtmlBody
{
    public string Value { get; }

    private EmailHtmlBody(string value)
    {
        Value = value;
    }

    // TODO: validation
    public static EmailHtmlBody Create(string value)
    {
        return new EmailHtmlBody(value);
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailHtmlBody other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}
