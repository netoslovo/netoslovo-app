using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace WordoGuessr.Common.Domain.ValueObjects;

public sealed partial record Word
{
    private const int MaxLength = 25;

    public string Text { get; }

    private Word(string text)
    {
        Text = text;
    }
    public static Word Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Word cannot be empty.", nameof(text));

        if (!ValidationRegex().IsMatch(text))
            throw new ArgumentException("Word contains invalid characters.", nameof(text));

        var normalized = Normalize(text);

        if (normalized.Length > MaxLength)
            throw new ArgumentException($"Word length cannot exceed {MaxLength}.", nameof(text));

        return new Word(normalized);
    }

    public static bool TryCreate(string text, [NotNullWhen(true)] out Word? word)
    {
        word = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!ValidationRegex().IsMatch(text))
            return false;

        var normalized = Normalize(text);

        if (normalized.Length > MaxLength)
            return false;

        word = new Word(normalized);
        return true;
    }

    private static string Normalize(string text) =>
        text.Trim().ToLowerInvariant().Replace('ё', 'е');

    public override string ToString() => Text;

    [GeneratedRegex(@"^[ ]*[а-яА-ЯёЁ-]+[ ]*$")]
    private static partial Regex ValidationRegex();
}
