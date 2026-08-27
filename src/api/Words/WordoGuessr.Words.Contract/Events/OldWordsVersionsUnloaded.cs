namespace WordoGuessr.Words.Contract.Events;

public sealed record OldWordsVersionsUnloaded(Guid SagaId, int WordsVersion, DateTimeOffset At);
