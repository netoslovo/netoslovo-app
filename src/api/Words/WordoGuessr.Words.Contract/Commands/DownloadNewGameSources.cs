namespace WordoGuessr.Words.Contract.Commands;

public sealed record DownloadNewGameSources(Guid SagaId, int WordsVersion);