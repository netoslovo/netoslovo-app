namespace WordoGuessr.Game.Contract.Commands;

public sealed record UpdateSingleGameResults(Guid SagaId, int WordsVersion);
