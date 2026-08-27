namespace WordoGuessr.Words.Contract.Commands;

public sealed record ActivateNewWordsVersion(Guid SagaId, int WordsVersion);