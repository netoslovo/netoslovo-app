using Microsoft.EntityFrameworkCore;
using Wolverine;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.Contract.Messages.UploadWordsVersionSaga.Commands;

namespace WordoGuessr.Sagas.App.UseCases.CancelWordsVersionUpload;

internal sealed class CancelWordsVersionUploadHandler
    : ICommandHandler<CancelWordsVersionUploadCommand, Result<CancelWordsVersionUploadError>>
{
    private readonly ISagasStore _store;
    private readonly IMessageContext _messageContext;

    public CancelWordsVersionUploadHandler(ISagasStore store, IMessageContext messageContext)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _messageContext = messageContext ?? throw new ArgumentNullException(nameof(messageContext));
    }

    public async Task<Result<CancelWordsVersionUploadError>> Handle(
        CancelWordsVersionUploadCommand command,
        CancellationToken ct = default)
    {
        var sagaExists = await _store.UploadWordsVersionSagas
            .AnyAsync(saga => saga.SagaId == command.SagaId, ct);

        if (!sagaExists)
        {
            return Result<CancelWordsVersionUploadError>.Failure(CancelWordsVersionUploadError.NotFound);
        }

        var sagaCommand = new CancelUploadWordsVersionSaga(command.SagaId);
        await _messageContext.SendAsync(sagaCommand);
        return Result<CancelWordsVersionUploadError>.Success();
    }
}
