namespace WordoGuessr.Words.Contract.Events;

public sealed record WordsIndexesInserted(Guid SagaId, int WordsVersion, int TotalWordsCount, DateTimeOffset At);
