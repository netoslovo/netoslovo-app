using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Words.App;

public sealed class WordsModule : IWordsModule
{
    private readonly IWordsDistanceStore _wordsDistanceStore;

    public WordsModule(IWordsDistanceStore wordsDistanceStore)
    {
        _wordsDistanceStore = wordsDistanceStore ?? throw new ArgumentNullException(nameof(wordsDistanceStore));
    }

    public Task<WordsData> LoadData(WordsDataRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _wordsDistanceStore.GetData(request, ct);
    }
}
