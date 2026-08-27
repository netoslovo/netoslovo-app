using Microsoft.EntityFrameworkCore;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;

namespace WordoGuessr.Sagas.App.Abstractions;

public interface ISagasStore
{
    DbSet<UploadWordsVersionSaga> UploadWordsVersionSagas { get; }
    DbSet<UploadWordsVersionSagaHistory> UploadWordsVersionSagasHistory { get; }
}
