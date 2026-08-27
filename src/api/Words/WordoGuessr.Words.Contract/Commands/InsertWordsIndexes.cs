namespace WordoGuessr.Words.Contract.Commands;

public sealed record InsertWordsIndexes(Guid SagaId, int WordsVersion);