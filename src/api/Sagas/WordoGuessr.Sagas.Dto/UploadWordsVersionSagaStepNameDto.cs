namespace WordoGuessr.Sagas.Dto;

public enum UploadWordsVersionSagaStepNameDto
{
    CreateNewWordsVersion,
    InsertWordsIndexes,
    InsertWordDistanceMaps,
    DownloadNewGameSources,
    UploadNewGameSourcesVersion,
    ActivateNewWordsVersion,
    WaitingForPostActivationTasks,
    UpdateSingleGames,
    UpdateSingleGameResults,
    InvalidateWordsCache,
    UnloadOldWordsVersions,
    Cancel,
    Timeout
}
