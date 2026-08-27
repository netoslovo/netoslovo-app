using WordoGuessr.Email.Domain;

namespace WordoGuessr.Email.Infra.EmailSending;

internal interface IEmailQueueInternal
{
    Task<IReadOnlyList<EmailMessage>> GetNextBatch(int batchSize, CancellationToken ct = default);
    Task Acknowledge(
        Guid messageId,
        uint messageVersion,
        EmailMessageState state,
        DateTimeOffset at,
        CancellationToken ct = default);

    Task<EmailQueueReadyMetrics> GetReadyMessagesMetrics(CancellationToken ct = default);
    Task<int> GetTotalMessagesCount(CancellationToken ct = default);

    Task ArchiveMessagesInTerminationStates(int batchSize, CancellationToken ct = default);
    Task ClearExpiredDeduplicationEntries(CancellationToken ct = default);
}

internal sealed record EmailQueueReadyMetrics(
    int TotalReady,
    DateTimeOffset? OldestReady);
