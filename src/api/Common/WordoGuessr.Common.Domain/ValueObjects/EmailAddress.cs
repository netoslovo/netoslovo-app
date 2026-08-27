using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace WordoGuessr.Common.Domain.ValueObjects;

public sealed class EmailAddress
{
    private const int MinEmailLength = 3;
    private const int MaxEmailLength = 254;
    private const int MaxEmailLocalPartLength = 64;

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (!TryNormalizeAndValidate(value, out var normalized))
        {
            throw new ArgumentException($"Email address value '{value}' is invalid", nameof(value));
        }

        return new EmailAddress(normalized);
    }

    public static bool TryCreate(
        string value,
        [NotNullWhen(true)] out EmailAddress? emailAddress)
    {
        if (!TryNormalizeAndValidate(value, out var normalized))
        {
            emailAddress = null;
            return false;
        }

        emailAddress = new EmailAddress(normalized);
        return true;
    }

    public static bool IsValid(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return IsValidAsIs(value);
    }

    public string GetLocalPart()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex > 0 ? Value[..atIndex] : Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailAddress other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    private static bool TryNormalizeAndValidate(
        string value,
        [NotNullWhen(true)] out string? normalized)
    {
        ArgumentNullException.ThrowIfNull(value);

        normalized = value.Trim();

        if (!IsValidAsIs(normalized))
        {
            normalized = null;
            return false;
        }

        return true;
    }

    private static bool IsValidAsIs(string email)
    {
        if (email.Length < MinEmailLength || email.Length > MaxEmailLength)
            return false;

        var atIndex = email.IndexOf('@');

        if (atIndex <= 0)
            return false;

        if (atIndex != email.LastIndexOf('@'))
            return false;

        if (atIndex > MaxEmailLocalPartLength)
            return false;

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
