namespace WordoGuessr.Game.Contract.Events;

public sealed record NewGameSourcesVersionUploaded(Guid SagaId, int WordsVersion, DateTimeOffset At);
