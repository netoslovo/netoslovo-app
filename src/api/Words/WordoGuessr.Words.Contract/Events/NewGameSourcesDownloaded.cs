using WordoGuessr.Common.Dto;

namespace WordoGuessr.Words.Contract.Events;

public sealed record NewGameSourcesDownloaded(
    Guid SagaId,
    int WordsVersion,
    GameSourceDto[] GameSources,
    DateTimeOffset At);
