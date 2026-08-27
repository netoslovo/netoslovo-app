using WordoGuessr.Words.Contract;

namespace WordoGuessr.Words.App.Abstractions;

public interface IWordsDistanceStore
{
    Task<WordsData> GetData(WordsDataRequest request, CancellationToken ct = default);
}
