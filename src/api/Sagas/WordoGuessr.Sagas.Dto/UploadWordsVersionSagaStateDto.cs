namespace WordoGuessr.Sagas.Dto;

public enum UploadWordsVersionSagaStateDto
{
    Active,
    Completed,
    Canceled,
    Timeout,
    Error
}
