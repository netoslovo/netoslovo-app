using System.Diagnostics.CodeAnalysis;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilterContext
{
    private sealed record Replacement(string Value, int SourceCharsTaken);

    public int Version { get; }

    private readonly TrieNode _trieStartNode = new TrieNode();
    private Dictionary<string, string[]> _replaceableTrigrams = [];
    private Dictionary<string, string[]> _replaceableBigrams = [];
    private Dictionary<char, string[]> _replaceableCharacters = [];

    public UserNameFilterContext(int version)
    {
        Version = version;
    }

    public void AddWord(string word)
    {
        word = word.ToLowerInvariant();
        if (string.IsNullOrEmpty(word))
        {
            throw new ArgumentException("Word couldn't be null or empty", nameof(word));
        }
        var node = _trieStartNode;
        for (int i = 0; i < word.Length; i++)
        {
            var character = word[i];
            if (!node.ChildNodes.TryGetValue(character, out var childNode))
            {
                childNode = new TrieNode();
                node.ChildNodes[character] = childNode;
            }

            node = childNode;
        }

        node.IsTerminal = true;
    }

    public void LoadReplacements(IReadOnlyList<UserNameReplacement> replacements)
    {
        var trigrams = new Dictionary<string, List<string>>();
        var bigrams = new Dictionary<string, List<string>>();
        var characters = new Dictionary<char, List<string>>();

        foreach (var replacement in replacements)
        {
            AddReplacement(replacement, trigrams, bigrams, characters);
        }

        _replaceableTrigrams = ToArrayDictionary(trigrams);
        _replaceableBigrams = ToArrayDictionary(bigrams);
        _replaceableCharacters = characters.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Distinct().ToArray());
    }

    public bool IsSafe(string userName)
    {
        for (int i = 0; i < userName.Length; i++)
        {
            if (!AllPossibleOptionsAreSafe(userName, i, _trieStartNode))
            {
                return false;
            }
        }

        return true;
    }

    private bool AllPossibleOptionsAreSafe(string userName, int startPos, TrieNode currentNode)
    {
        if (startPos >= userName.Length)
        {
            return true;
        }

        var options = GetPossibleOptions(userName, startPos);

        foreach (var option in options)
        {
            var moveResult = currentNode.TryMove(option.Value, out var newNode);
            switch (moveResult)
            {
                case TryMoveResult.UnsafeWordFound:
                    return false;

                case TryMoveResult.BrokenChain:
                    continue;

                case TryMoveResult.Moved:
                    var newPos = startPos + option.SourceCharsTaken;
                    if (!AllPossibleOptionsAreSafe(userName, newPos, newNode!))
                    {
                        return false;
                    }
                    break;

                default: throw new InvalidOperationException($"Unknown {typeof(TryMoveResult)}: {moveResult}");
            }
        }

        return true;
    }

    private bool IsReplaceableTrigram(string trigram, out string[] replacements)
    {
        return _replaceableTrigrams.TryGetValue(trigram, out replacements!);
    }

    private bool IsReplaceableBigram(string bigram, out string[] replacements)
    {
        return _replaceableBigrams.TryGetValue(bigram, out replacements!);
    }

    private bool IsReplaceableCharacter(char character, out string[] replacements)
    {
        return _replaceableCharacters.TryGetValue(character, out replacements!);
    }

    private IEnumerable<Replacement> GetPossibleOptions(string word, int pos)
    {
        var currentChar = word[pos];
        var remainingLength = word.Length - pos;

        if (remainingLength >= 3)
        {
            var trigram = word.Substring(pos, 3);
            if (IsReplaceableTrigram(trigram, out var trigramReplacements))
            {
                foreach (var replacement in trigramReplacements)
                {
                    yield return new Replacement(replacement, 3);
                }
            }

        }
        if (TryGetSeparatedGram(word, pos, 3, out var sepTrigramReplacement) &&
            IsReplaceableTrigram(sepTrigramReplacement.Value, out var separatedTrigramReplacements))
        {
            foreach (var replacement in separatedTrigramReplacements)
            {
                yield return new Replacement(replacement, sepTrigramReplacement.SourceCharsTaken);
            }
        }

        if (remainingLength >= 2)
        {
            var bigram = word.Substring(pos, 2);
            if (IsReplaceableBigram(bigram, out var bigramReplacements))
            {
                foreach (var replacement in bigramReplacements)
                {
                    yield return new Replacement(replacement, 2);
                }
            }

            int repeatedLength = 1;
            for (int i = pos + 1; i < word.Length; i++)
            {
                if (word[i] != currentChar)
                {
                    break;
                }

                repeatedLength++;
            }

            if (repeatedLength > 1)
            {
                yield return new Replacement(currentChar.ToString(), repeatedLength);
                if (IsReplaceableCharacter(currentChar, out var repeatedCharReplacements))
                {
                    foreach (var replacement in repeatedCharReplacements)
                    {
                        yield return new Replacement(replacement, repeatedLength);
                    }
                }
            }
        }

        if (TryGetSeparatedGram(word, pos, 2, out var sepBigramReplacement) &&
            IsReplaceableBigram(sepBigramReplacement.Value, out var separatedBigramReplacements))
        {
            foreach (var replacement in separatedBigramReplacements)
            {
                yield return new Replacement(replacement, sepBigramReplacement.SourceCharsTaken);
            }
        }

        if (TryGetSeparatedRepeat(word, pos, out var separatedRepeatSourceCharsTaken))
        {
            yield return new Replacement(currentChar.ToString(), separatedRepeatSourceCharsTaken);
            if (IsReplaceableCharacter(currentChar, out var separatedRepeatReplacements))
            {
                foreach (var replacement in separatedRepeatReplacements)
                {
                    yield return new Replacement(replacement, separatedRepeatSourceCharsTaken);
                }
            }
        }
        foreach (var equivalentRepeatOption in GetEquivalentRepeatOptions(word, pos))
        {
            yield return equivalentRepeatOption;
        }

        var singleCharacterOptions = GetCharacterOptions(currentChar);
        foreach (var option in singleCharacterOptions)
        {
            yield return new Replacement(option, 1);
        }
    }

    private static bool TryGetSeparatedGram(
        string word,
        int pos,
        int length,
        [NotNullWhen(true)]
        out Replacement? replacement)
    {
        if (IsSkippableCharacter(word[pos]))
        {
            replacement = null;
            return false;
        }

        var gram = "";
        var sourceCharsTaken = 0;

        for (int i = pos; i < word.Length && gram.Length < length; i++)
        {
            if (IsSkippableCharacter(word[i]))
            {
                continue;
            }

            gram += word[i];
            sourceCharsTaken = i - pos + 1;
        }

        if (gram.Length == length && sourceCharsTaken > length)
        {
            replacement = new Replacement(gram, sourceCharsTaken);
            return true;
        }
        else
        {
            replacement = null;
            return false;
        }
    }

    private static bool TryGetSeparatedRepeat(
        string word,
        int pos,
        out int sourceCharsTaken)
    {
        sourceCharsTaken = 0;

        var currentChar = word[pos];
        if (IsSkippableCharacter(currentChar))
        {
            return false;
        }

        var repeatedChars = 1;
        for (int i = pos + 1; i < word.Length; i++)
        {
            if (IsSkippableCharacter(word[i]))
            {
                continue;
            }

            if (word[i] != currentChar)
            {
                break;
            }

            repeatedChars++;
            sourceCharsTaken = i - pos + 1;
        }

        return repeatedChars > 1 && sourceCharsTaken > repeatedChars;
    }

    private IEnumerable<Replacement> GetEquivalentRepeatOptions(string word, int pos)
    {
        if (IsSkippableCharacter(word[pos]))
        {
            yield break;
        }

        var commonOptions = GetCharacterOptions(word[pos]).ToHashSet();
        var repeatedChars = 1;
        var sourceCharsTaken = 0;
        for (int i = pos + 1; i < word.Length; i++)
        {
            if (IsSkippableCharacter(word[i]))
            {
                continue;
            }

            var nextCommonOptions = commonOptions
                .Intersect(GetCharacterOptions(word[i]))
                .ToHashSet();
            if (nextCommonOptions.Count == 0)
            {
                break;
            }

            commonOptions = nextCommonOptions;
            repeatedChars++;
            sourceCharsTaken = i - pos + 1;
        }

        if (repeatedChars <= 1)
        {
            yield break;
        }

        foreach (var option in commonOptions)
        {
            yield return new Replacement(option, sourceCharsTaken);
        }
    }

    private IEnumerable<string> GetCharacterOptions(char character)
    {
        yield return character.ToString();
        if (!IsReplaceableCharacter(character, out var replacements))
        {
            yield break;
        }

        foreach (var replacement in replacements)
        {
            yield return replacement;
        }
    }

    private static bool IsSkippableCharacter(char character)
    {
        return character is '-' or '_' or '.';
    }

    private static void AddReplacement(
        UserNameReplacement replacement,
        Dictionary<string, List<string>> trigrams,
        Dictionary<string, List<string>> bigrams,
        Dictionary<char, List<string>> characters)
    {
        var source = replacement.Source.ToLowerInvariant();
        var replacementValue = replacement.Replacement.ToLowerInvariant();

        if (string.IsNullOrEmpty(source))
        {
            throw new InvalidOperationException("User name replacement source couldn't be null or empty");
        }

        switch (source.Length)
        {
            case 1:
                AddReplacement(characters, source[0], replacementValue);
                break;

            case 2:
                AddReplacement(bigrams, source, replacementValue);
                break;

            case 3:
                AddReplacement(trigrams, source, replacementValue);
                break;

            default:
                throw new InvalidOperationException(
                    $"User name replacement source '{source}' should contain from 1 to 3 characters");
        }
    }

    private static void AddReplacement<TKey>(
        Dictionary<TKey, List<string>> replacements,
        TKey source,
        string replacement)
        where TKey : notnull
    {
        if (!replacements.TryGetValue(source, out var values))
        {
            values = new List<string>();
            replacements[source] = values;
        }

        values.Add(replacement);
    }

    private static Dictionary<string, string[]> ToArrayDictionary(Dictionary<string, List<string>> replacements) =>
    replacements.ToDictionary(
        pair => pair.Key,
        pair => pair.Value.Distinct().ToArray());
}