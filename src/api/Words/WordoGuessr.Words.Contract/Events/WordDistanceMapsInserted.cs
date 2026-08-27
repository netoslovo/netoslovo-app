namespace WordoGuessr.Words.Contract.Events;

public sealed record WordDistanceMapsInserted(Guid SagaId, int WordsVersion, DateTimeOffset At);
