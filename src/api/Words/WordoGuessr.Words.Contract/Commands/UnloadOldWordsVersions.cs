namespace WordoGuessr.Words.Contract.Commands;

public sealed record UnloadOldWordsVersions(Guid SagaId, int WordsVersion);