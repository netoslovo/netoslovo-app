namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public enum UploadWordsVersionSagaStepName
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
