namespace WordoGuessr.Words.Contract.Commands;

public sealed record InsertWordDistanceMaps(Guid SagaId, int WordsVersion, int TotalWordsCount);