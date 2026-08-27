namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public sealed class UploadWordsVersionSagaHistory
{
    public Guid SagaId { get; }
    public int WordsVersion { get; }
    public UploadWordsVersionSagaState State { get; }
    public UploadWordsVersionSagaStepName CurrentStep { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; }

    private readonly List<UploadWordsVersionSagaCompletedStep> _completedSteps
        = [];
    public IList<UploadWordsVersionSagaCompletedStep> CompletedSteps =>
        _completedSteps.AsReadOnly();

    public int Version { get; }

    private UploadWordsVersionSagaHistory()
    {
    }

    public static UploadWordsVersionSagaHistory FromSaga(UploadWordsVersionSaga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        return new UploadWordsVersionSagaHistory(
            saga.SagaId,
            saga.WordsVersion,
            saga.State,
            saga.CurrentStep,
            saga.CreatedAt,
            saga.CompletedAt,
            saga.CompletedSteps.ToArray(),
            saga.Version);
    }

    public UploadWordsVersionSagaHistory(
        Guid sagaId,
        int wordsVersion,
        UploadWordsVersionSagaState state,
        UploadWordsVersionSagaStepName currentStep,
        DateTimeOffset createdAt,
        DateTimeOffset? completedAt,
        IReadOnlyList<UploadWordsVersionSagaCompletedStep> completedSteps,
        int version)
    {
        SagaId = sagaId;
        WordsVersion = wordsVersion;
        State = state;
        CurrentStep = currentStep;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        _completedSteps.AddRange(completedSteps);
        Version = version;

    }
}
