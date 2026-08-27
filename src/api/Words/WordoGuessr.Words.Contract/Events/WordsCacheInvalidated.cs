namespace WordoGuessr.Words.Contract.Events;

public sealed record WordsCacheInvalidated(Guid SagaId, int WordsVersion, DateTimeOffset At);
