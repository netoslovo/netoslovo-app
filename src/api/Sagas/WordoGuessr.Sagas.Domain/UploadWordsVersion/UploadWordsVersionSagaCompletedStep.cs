namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public sealed class UploadWordsVersionSagaCompletedStep
{
    public UploadWordsVersionSagaStepName Name { get; }
    public DateTimeOffset CompletedAt { get; }
    public bool IsSuccess { get; }

    private UploadWordsVersionSagaCompletedStep() { }

    private UploadWordsVersionSagaCompletedStep(
        UploadWordsVersionSagaStepName name,
        DateTimeOffset completedAt,
        bool success)
    {
        Name = name;
        CompletedAt = completedAt;
        IsSuccess = success;
    }

    public static UploadWordsVersionSagaCompletedStep Success(
        UploadWordsVersionSagaStepName name,
        DateTimeOffset completedAt) =>
        new(name, completedAt, true);

    public static UploadWordsVersionSagaCompletedStep Failure(
        UploadWordsVersionSagaStepName name,
        DateTimeOffset completedAt) =>
        new(name, completedAt, false);
}
