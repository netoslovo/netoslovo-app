namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public enum UploadWordsVersionSagaState
{
    Active,
    Completed,
    Canceled,
    Timeout,
    Error
}
