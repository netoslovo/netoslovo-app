namespace WordoGuessr.Words.Contract.Commands;

public sealed record CreateNewWordsVersion(Guid SagaId, int WordsVersion);