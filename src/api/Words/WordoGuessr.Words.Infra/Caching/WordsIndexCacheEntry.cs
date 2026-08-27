using MemoryPack;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Words.Infra.Helpers;
using WordoGuessr.Words.Infra.WordsDistanceStorage;

namespace WordoGuessr.Words.Infra.Caching;

[MemoryPackable]
internal sealed partial class WordsIndexCacheEntry
{
    public int Version { get; }
    public string[] WordsById { get; }
    public Dictionary<string, ushort> WordIdsByText { get; }
    public ushort TotalWords => Converters.ToUshort(WordsById.Length);

    [MemoryPackConstructor]
    public WordsIndexCacheEntry(int version, string[] wordsById, Dictionary<string, ushort> wordIdsByText)
    {
        Version = version;
        WordsById = wordsById;
        WordIdsByText = wordIdsByText;
    }

    public WordsIndexCacheEntry(int version, Word[] wordsById, Dictionary<Word, ushort> wordIdsByText)
        : this(
            version,
            wordsById.Select(w => w.Text).ToArray(),
            wordIdsByText.ToDictionary(w => w.Key.Text, w => w.Value))
    {
    }

    public Word GetWordById(ushort id)
    {
        if (id >= WordsById.Length)
        {
            throw new WordDistancesStoreException($"Id {id} is out of index range.");
        }

        var wordText = WordsById[id];
        if (!Word.TryCreate(wordText, out var word))
        {
            throw new WordDistancesStoreException(
                $"Words index contains invalid word text '{wordText}'.");
        }

        return word;
    }

    public ushort GetWordId(string word)
    {
        if (!WordIdsByText.TryGetValue(word, out var id))
        {
            throw new WordDistancesStoreException($"Word {word} not found in the index");
        }

        return id;
    }

    public ushort GetWordId(Word word)
    {
        if (!WordIdsByText.TryGetValue(word.Text, out var id))
        {
            throw new WordDistancesStoreException($"Word {word} not found in the index");
        }

        return id;
    }
}
