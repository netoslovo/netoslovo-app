using Microsoft.EntityFrameworkCore;
using Wolverine;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.Contract.Messages.UploadWordsVersionSaga.Commands;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;

namespace WordoGuessr.Sagas.App.UseCases.UploadWordsVersion;

internal sealed class UploadWordsVersionHandler
    : ICommandHandler<UploadWordsVersionCommand, Result<UploadWordsVersionError>>
{
    private readonly ISagasStore _store;
    private readonly IMessageContext _messageContext;

    public UploadWordsVersionHandler(ISagasStore store, IMessageContext messageContext)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _messageContext = messageContext ?? throw new ArgumentNullException(nameof(messageContext));
    }

    public async Task<Result<UploadWordsVersionError>> Handle(
        UploadWordsVersionCommand command,
        CancellationToken ct = default)
    {
        var isUploadRunning = await _store.UploadWordsVersionSagas
            .AnyAsync(saga => saga.State == UploadWordsVersionSagaState.Active, ct);

        if (isUploadRunning)
        {
            return Result<UploadWordsVersionError>.Failure(UploadWordsVersionError.AlreadyRunning);
        }

        var sagaCommand = new StartUploadWordsVersionSaga(
            command.SagaId,
            command.Version);

        await _messageContext.SendAsync(sagaCommand);
        return Result<UploadWordsVersionError>.Success();
    }
}
