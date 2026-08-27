namespace WordoGuessr.Words.Contract.Events;

public sealed record NewWordsVersionActivated(Guid SagaId, int WordsVersion, DateTimeOffset At);
