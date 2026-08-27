namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

public sealed class WordDistancesStoreException : Exception
{
    public WordDistancesStoreException() : base() { }

    public WordDistancesStoreException(string message) : base(message) { }

    public WordDistancesStoreException(string message, Exception inner) : base(message, inner) { }
}
