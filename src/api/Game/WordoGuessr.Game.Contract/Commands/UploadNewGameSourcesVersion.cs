using WordoGuessr.Common.Dto;

namespace WordoGuessr.Game.Contract.Commands;

public sealed record UploadNewGameSourcesVersion(
    Guid SagaId,
    int WordsVersion,
    GameSourceDto[] GameSources);
