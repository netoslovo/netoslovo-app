namespace WordoGuessr.Sagas.Contract.Messages.UploadWordsVersionSaga.Commands;

public sealed record StartUploadWordsVersionSaga(Guid SagaId, int WordsVersion);
