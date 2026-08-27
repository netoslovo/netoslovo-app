using JasperFx;
using Wolverine;
using WordoGuessr.Game.Contract.Commands;
using WordoGuessr.Game.Contract.Events;
using WordoGuessr.Sagas.Contract.Messages.UploadWordsVersionSaga.Commands;
using WordoGuessr.Sagas.Domain.UploadWordsVersion.Events;
using WordoGuessr.Words.Contract.Commands;
using WordoGuessr.Words.Contract.Events;

namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public sealed class UploadWordsVersionSaga : Saga, IRevisioned
{
    public Guid SagaId { get; }
    public int WordsVersion { get; }
    public UploadWordsVersionSagaState State { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public UploadWordsVersionSagaStepName CurrentStep { get; private set; }

    private readonly List<UploadWordsVersionSagaCompletedStep> _completedSteps = [];
    public IList<UploadWordsVersionSagaCompletedStep> CompletedSteps =>
        _completedSteps.AsReadOnly();

    private static readonly UploadWordsVersionSagaStepName[] _sagaCompletionSteps =
    [
        UploadWordsVersionSagaStepName.InvalidateWordsCache,
        UploadWordsVersionSagaStepName.UnloadOldWordsVersions,
        UploadWordsVersionSagaStepName.UpdateSingleGames,
        UploadWordsVersionSagaStepName.UpdateSingleGameResults
    ];

    public UploadWordsVersionSaga(Guid sagaId, int wordsVersion, DateTimeOffset createdAt)
    {
        SagaId = sagaId;
        WordsVersion = wordsVersion;
        State = UploadWordsVersionSagaState.Active;
        CreatedAt = createdAt;
    }

    public static (
        UploadWordsVersionSaga,
        CreateNewWordsVersion,
        UploadWordsVersionSagaTimeout)
        Start(StartUploadWordsVersionSaga startCommand, TimeProvider timeProvider)
    {
        var saga = new UploadWordsVersionSaga(
            startCommand.SagaId,
            startCommand.WordsVersion,
            timeProvider.GetUtcNow())
        {
            CurrentStep = UploadWordsVersionSagaStepName.CreateNewWordsVersion
        };

        var createWordsVersionCommand = new CreateNewWordsVersion(startCommand.SagaId, startCommand.WordsVersion);
        var timeout = new UploadWordsVersionSagaTimeout(startCommand.SagaId);

        return (saga, createWordsVersionCommand, timeout);
    }

    public InsertWordsIndexes Handle(NewWordsVersionCreated @event)
    {
        EnsureExpectedStepForEvent<NewWordsVersionCreated>(UploadWordsVersionSagaStepName.CreateNewWordsVersion);

        SuccessStep(
            UploadWordsVersionSagaStepName.CreateNewWordsVersion,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.InsertWordsIndexes;
        return new InsertWordsIndexes(SagaId, WordsVersion);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<CreateNewWordsVersion> @event)
    {
        EnsureExpectedStepForFaultCommand<CreateNewWordsVersion>(UploadWordsVersionSagaStepName.CreateNewWordsVersion);

        FailStep(
            UploadWordsVersionSagaStepName.CreateNewWordsVersion,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public InsertWordDistanceMaps Handle(WordsIndexesInserted @event)
    {
        EnsureExpectedStepForEvent<WordsIndexesInserted>(UploadWordsVersionSagaStepName.InsertWordsIndexes);

        SuccessStep(
            UploadWordsVersionSagaStepName.InsertWordsIndexes,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.InsertWordDistanceMaps;
        return new InsertWordDistanceMaps(SagaId, WordsVersion, @event.TotalWordsCount);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<InsertWordsIndexes> @event)
    {
        EnsureExpectedStepForFaultCommand<InsertWordsIndexes>(UploadWordsVersionSagaStepName.InsertWordsIndexes);

        FailStep(
            UploadWordsVersionSagaStepName.InsertWordsIndexes,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public DownloadNewGameSources Handle(WordDistanceMapsInserted @event)
    {
        EnsureExpectedStepForEvent<WordDistanceMapsInserted>(UploadWordsVersionSagaStepName.InsertWordDistanceMaps);

        SuccessStep(
            UploadWordsVersionSagaStepName.InsertWordDistanceMaps,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.DownloadNewGameSources;
        return new DownloadNewGameSources(SagaId, WordsVersion);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<InsertWordDistanceMaps> @event)
    {
        EnsureExpectedStepForFaultCommand<InsertWordDistanceMaps>(UploadWordsVersionSagaStepName.InsertWordDistanceMaps);

        FailStep(
            UploadWordsVersionSagaStepName.InsertWordDistanceMaps,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadNewGameSourcesVersion Handle(NewGameSourcesDownloaded @event)
    {
        EnsureExpectedStepForEvent<NewGameSourcesDownloaded>(UploadWordsVersionSagaStepName.DownloadNewGameSources);

        SuccessStep(
            UploadWordsVersionSagaStepName.DownloadNewGameSources,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion;
        return new UploadNewGameSourcesVersion(SagaId, WordsVersion, @event.GameSources);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<DownloadNewGameSources> @event)
    {
        EnsureExpectedStepForFaultCommand<DownloadNewGameSources>(UploadWordsVersionSagaStepName.DownloadNewGameSources);

        FailStep(
            UploadWordsVersionSagaStepName.DownloadNewGameSources,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public ActivateNewWordsVersion Handle(NewGameSourcesVersionUploaded @event)
    {
        EnsureExpectedStepForEvent<NewGameSourcesVersionUploaded>(UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion);

        SuccessStep(
            UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.ActivateNewWordsVersion;

        return new ActivateNewWordsVersion(SagaId, WordsVersion);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<UploadNewGameSourcesVersion> @event)
    {
        EnsureExpectedStepForFaultCommand<UploadNewGameSourcesVersion>(UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion);

        FailStep(
            UploadWordsVersionSagaStepName.UploadNewGameSourcesVersion,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public (UpdateSingleGames, UpdateSingleGameResults, InvalidateWordsCache, UnloadOldWordsVersions) Handle(NewWordsVersionActivated @event)
    {
        EnsureExpectedStepForEvent<NewWordsVersionActivated>(UploadWordsVersionSagaStepName.ActivateNewWordsVersion);

        SuccessStep(
            UploadWordsVersionSagaStepName.ActivateNewWordsVersion,
            @event.At);

        CurrentStep = UploadWordsVersionSagaStepName.WaitingForPostActivationTasks;

        return (
            new UpdateSingleGames(SagaId, WordsVersion),
            new UpdateSingleGameResults(SagaId, WordsVersion),
            new InvalidateWordsCache(SagaId, WordsVersion),
            new UnloadOldWordsVersions(SagaId, WordsVersion)
        );
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<ActivateNewWordsVersion> @event)
    {
        EnsureExpectedStepForFaultCommand<ActivateNewWordsVersion>(UploadWordsVersionSagaStepName.ActivateNewWordsVersion);

        FailStep(
            UploadWordsVersionSagaStepName.ActivateNewWordsVersion,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadWordsVersionSagaCompleted? Handle(WordsCacheInvalidated @event)
    {
        EnsureExpectedStepForEvent<WordsCacheInvalidated>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        return TryCompletePostActivationTasksWaiting(
            UploadWordsVersionSagaStepName.InvalidateWordsCache,
            @event.At);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<InvalidateWordsCache> @event)
    {
        EnsureExpectedStepForFaultCommand<InvalidateWordsCache>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        FailStep(
            UploadWordsVersionSagaStepName.InvalidateWordsCache,
            @event.FailedAt);

        FailStep(
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadWordsVersionSagaCompleted? Handle(OldWordsVersionsUnloaded @event)
    {
        EnsureExpectedStepForEvent<OldWordsVersionsUnloaded>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        return TryCompletePostActivationTasksWaiting(
            UploadWordsVersionSagaStepName.UnloadOldWordsVersions,
            @event.At);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<UnloadOldWordsVersions> @event)
    {
        EnsureExpectedStepForFaultCommand<UnloadOldWordsVersions>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        FailStep(
            UploadWordsVersionSagaStepName.UnloadOldWordsVersions,
            @event.FailedAt);

        FailStep(
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadWordsVersionSagaCompleted? Handle(SingleGamesUpdated @event)
    {
        EnsureExpectedStepForEvent<SingleGamesUpdated>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        return TryCompletePostActivationTasksWaiting(
            UploadWordsVersionSagaStepName.UpdateSingleGames,
            @event.At);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<UpdateSingleGames> @event)
    {
        EnsureExpectedStepForFaultCommand<UpdateSingleGames>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        FailStep(
            UploadWordsVersionSagaStepName.UpdateSingleGames,
            @event.FailedAt);

        FailStep(
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadWordsVersionSagaCompleted? Handle(SingleGameResultsUpdated @event)
    {
        EnsureExpectedStepForEvent<SingleGameResultsUpdated>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        return TryCompletePostActivationTasksWaiting(
            UploadWordsVersionSagaStepName.UpdateSingleGameResults,
            @event.At);
    }

    public UploadWordsVersionSagaCompleted Handle(Fault<UpdateSingleGameResults> @event)
    {
        EnsureExpectedStepForFaultCommand<UpdateSingleGameResults>(UploadWordsVersionSagaStepName.WaitingForPostActivationTasks);

        FailStep(
            UploadWordsVersionSagaStepName.UpdateSingleGameResults,
            @event.FailedAt);

        FailStep(
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks,
            @event.FailedAt);

        return ErrorSaga(@event.FailedAt);
    }

    public UploadWordsVersionSagaCompleted Handle(CancelUploadWordsVersionSaga _, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow();

        SuccessStep(
            UploadWordsVersionSagaStepName.Cancel,
            now);

        return CancelSaga(now);
    }

    public UploadWordsVersionSagaCompleted Handle(UploadWordsVersionSagaTimeout _, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow();

        SuccessStep(
            UploadWordsVersionSagaStepName.Timeout,
            now);

        return TimeoutSaga(now);
    }

    private UploadWordsVersionSagaCompleted? TryCompletePostActivationTasksWaiting(
        UploadWordsVersionSagaStepName stepName,
        DateTimeOffset completedAt)
    {
        SuccessStep(stepName, completedAt);
        if (!_sagaCompletionSteps.All(IsStepCompleted)) return null;

        SuccessStep(
            UploadWordsVersionSagaStepName.WaitingForPostActivationTasks,
            completedAt);

        return CompleteSaga(completedAt);
    }

    private bool IsStepCompleted(UploadWordsVersionSagaStepName name) =>
        _completedSteps.Any(step => step.Name == name);

    private void EnsureExpectedStepForEvent<TEvent>(UploadWordsVersionSagaStepName expectedStep)
    {
        if (CurrentStep == expectedStep) return;

        throw new UnexpectedMessageForStepException(typeof(TEvent), CurrentStep.ToString());
    }

    private void EnsureExpectedStepForFaultCommand<TCommand>(UploadWordsVersionSagaStepName expectedStep)
    {
        if (CurrentStep == expectedStep) return;

        throw new UnexpectedMessageForStepException(typeof(TCommand), CurrentStep.ToString());
    }

    private void SuccessStep(UploadWordsVersionSagaStepName name, DateTimeOffset completedAt) =>
        _completedSteps.Add(UploadWordsVersionSagaCompletedStep.Success(name, completedAt));

    private void FailStep(UploadWordsVersionSagaStepName name, DateTimeOffset completedAt) =>
        _completedSteps.Add(UploadWordsVersionSagaCompletedStep.Failure(name, completedAt));

    private UploadWordsVersionSagaCompleted CompleteSaga(DateTimeOffset at) =>
        CompleteSaga(UploadWordsVersionSagaState.Completed, at);

    private UploadWordsVersionSagaCompleted CancelSaga(DateTimeOffset at) =>
        CompleteSaga(UploadWordsVersionSagaState.Canceled, at);

    private UploadWordsVersionSagaCompleted TimeoutSaga(DateTimeOffset at) =>
        CompleteSaga(UploadWordsVersionSagaState.Timeout, at);

    private UploadWordsVersionSagaCompleted ErrorSaga(DateTimeOffset at) =>
        CompleteSaga(UploadWordsVersionSagaState.Error, at);

    private UploadWordsVersionSagaCompleted CompleteSaga(UploadWordsVersionSagaState withState, DateTimeOffset at)
    {
        State = withState;
        CompletedAt = at;
        MarkCompleted();
        return new UploadWordsVersionSagaCompleted(UploadWordsVersionSagaHistory.FromSaga(this));
    }
}
