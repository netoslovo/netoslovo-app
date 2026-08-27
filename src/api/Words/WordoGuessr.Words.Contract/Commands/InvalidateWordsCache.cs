namespace WordoGuessr.Words.Contract.Commands;

public sealed record InvalidateWordsCache(Guid SagaId, int WordsVersion);