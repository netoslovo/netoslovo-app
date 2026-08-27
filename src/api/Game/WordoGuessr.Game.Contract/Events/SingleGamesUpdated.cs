namespace WordoGuessr.Game.Contract.Events;

public sealed record SingleGamesUpdated(Guid SagaId, int WordsVersion, DateTimeOffset At);
