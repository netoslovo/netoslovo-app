using JasperFx.Core;
using Wolverine;

namespace WordoGuessr.Sagas.Domain.UploadWordsVersion;

public sealed record UploadWordsVersionSagaTimeout(Guid SagaId) : TimeoutMessage(30.Minutes());
