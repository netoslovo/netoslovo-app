using System.Diagnostics.CodeAnalysis;

namespace WordoGuessr.Common.Domain.ValueObjects;

public sealed class UserName
{
    public const int MinLength = 3;
    public const int MaxLength = 16;

    public const string AllowedCharacters =
        "abcdefghijklmnopqrstuvwxyz" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "0123456789" +
        "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
        "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
        "-._";

    public string Value { get; }

    private UserName(string value)
    {
        Value = value;
    }

    public static UserName Create(string value)
    {
        if (!TryNormalizeAndValidate(value, out var normalized))
        {
            throw new ArgumentException($"User name value '{value}' is invalid", nameof(value));
        }

        return new UserName(normalized);
    }

    public static bool TryCreate(
        string value,
        [NotNullWhen(true)] out UserName? userName)
    {
        if (!TryNormalizeAndValidate(value, out var normalized))
        {
            userName = null;
            return false;
        }

        userName = new UserName(normalized);
        return true;
    }

    public static bool IsValid(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return IsValidAsIs(value);
    }

    public static bool IsAllowedChar(char ch)
    {
        return AllowedCharacters.Contains(ch);
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

    private static bool IsValidAsIs(string value)
    {
        if (value.Length < MinLength || value.Length > MaxLength)
            return false;

        if (value.Any(ch => !IsAllowedChar(ch)))
            return false;

        return true;
    }

    public override string ToString() => Value;
}