namespace WordoGuessr.Words.Contract.Events;

public sealed record NewWordsVersionCreated(Guid SagaId, int WordsVersion, DateTimeOffset At);
