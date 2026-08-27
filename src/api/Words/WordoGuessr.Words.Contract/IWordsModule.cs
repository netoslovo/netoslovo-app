namespace WordoGuessr.Words.Contract;

public interface IWordsModule
{
    Task<WordsData> LoadData(WordsDataRequest request, CancellationToken ct = default);
}
