namespace WordoGuessr.Game.Contract.Commands;

public sealed record UpdateSingleGames(Guid SagaId, int WordsVersion);
