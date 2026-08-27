namespace WordoGuessr.Game.Contract.Events;

public sealed record SingleGameResultsUpdated(Guid SagaId, int WordsVersion, DateTimeOffset At);
