using WordoGuessr.Sagas.Domain.UploadWordsVersion;
using WordoGuessr.Sagas.Dto;

namespace WordoGuessr.Sagas.App.Mapping;

internal static class UploadWordsVersionSagaMapping
{
    public static UploadWordsVersionSagaDto MapToDto(this UploadWordsVersionSaga saga) =>
        new UploadWordsVersionSagaDto(
            saga.SagaId,
            saga.WordsVersion,
            saga.State.MapToDto(),
            saga.CurrentStep.MapToDto(),
            saga.CreatedAt,
            saga.CompletedAt,
            saga.CompletedSteps.Select(step => step.MapToDto()).ToArray(),
            saga.Version);

    public static UploadWordsVersionSagaHistoryDto MapToDto(this UploadWordsVersionSagaHistory saga) =>
        new UploadWordsVersionSagaHistoryDto(
            saga.SagaId,
            saga.WordsVersion,
            saga.State.MapToDto(),
            saga.CurrentStep.MapToDto(),
            saga.CreatedAt,
            saga.CompletedAt,
            saga.CompletedSteps.Select(step => step.MapToDto()).ToArray(),
            saga.Version);

    private static UploadWordsVersionSagaCompletedStepDto MapToDto(this UploadWordsVersionSagaCompletedStep step) =>
        new UploadWordsVersionSagaCompletedStepDto(
            step.Name.MapToDto(),
            step.IsSuccess,
            step.CompletedAt);

    private static UploadWordsVersionSagaStateDto MapToDto(this UploadWordsVersionSagaState state) =>
        state switch
        {
            UploadWordsVersionSagaState.Active => UploadWordsVersionSagaStateDto.Active,
            UploadWordsVersionSagaState.Completed => UploadWordsVersionSagaStateDto.Completed,
            UploadWordsVersionSagaState.Canceled => UploadWordsVersionSagaStateDto.Canceled,
            UploadWordsVersionSagaState.Timeout => UploadWordsVersionSagaStateDto.Timeout,
            UploadWordsVersionSagaState.Error => UploadWordsVersionSagaStateDto.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

    private static UploadWordsVersionSagaStepNameDto MapToDto(this UploadWordsVersionSagaStepName name) =>
        name switch
        {
            UploadWordsVersionSagaStepName.CreateNewWordsVersion => UploadWordsVersionSagaStepNameDto.CreateNewWordsVersion,
            UploadWordsVersionSagaStepName.InsertWordsIndexes => UploadWordsVersionSagaStepNameDto.InsertWordsIndexes,
            UploadWordsVersionSagaStepName.InsertWordDistanceMaps => UploadWordsVersionSagaStepNameDto.InsertWordDistanceMaps,
            UploadWordsVersionSagaStepName.DownloadNewGameSources => UploadWordsVersionSagaStepNameDto.DownloadNewGameSources,
            UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion => UploadWordsVersionSagaStepNameDto.UploadNewGameSourcesVersion,
            UploadWordsVersionSagaStepName.ActivateNewWordsVersion => UploadWordsVersionSagaStepNameDto.ActivateNewWordsVersion,
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks => UploadWordsVersionSagaStepNameDto.WaitingForPostActivationTasks,
            UploadWordsVersionSagaStepName.UpdateSingleGames => UploadWordsVersionSagaStepNameDto.UpdateSingleGames,
            UploadWordsVersionSagaStepName.UpdateSingleGameResults => UploadWordsVersionSagaStepNameDto.UpdateSingleGameResults,
            UploadWordsVersionSagaStepName.InvalidateWordsCache => UploadWordsVersionSagaStepNameDto.InvalidateWordsCache,
            UploadWordsVersionSagaStepName.UnloadOldWordsVersions => UploadWordsVersionSagaStepNameDto.UnloadOldWordsVersions,
            UploadWordsVersionSagaStepName.Cancel => UploadWordsVersionSagaStepNameDto.Cancel,
            UploadWordsVersionSagaStepName.Timeout => UploadWordsVersionSagaStepNameDto.Timeout,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
}
