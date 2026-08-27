using Microsoft.EntityFrameworkCore;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Email.App.Abstractions;
using WordoGuessr.Email.Domain;
using WordoGuessr.Email.Infra.EmailSending;

namespace WordoGuessr.Email.Infra.Database;

public sealed partial class EmailDbContext : IEmailQueue, IEmailQueueInternal
{
    private readonly EmailQueueOptions _emailQueueOptions;
    private readonly TimeProvider _timeProvider;

    public async Task<int> EnqueueMessage(EmailMessage message, CancellationToken ct = default)
    {
        var affected = await Database.ExecuteSqlAsync(
            $"""
            WITH deduplicatiton as (
                INSERT INTO email.messages_deduplication(source_id, created_at)
                VALUES ({message.SourceId}, {message.CreatedAt})
                ON CONFLICT (source_id) DO NOTHING
                RETURNING 1
            )
            INSERT INTO email.messages (
                id,
                "to",
                subject,
                body,
                created_at,
                updated_at,
                expire_at,
                attempts,
                state,
                source_id
            )
            SELECT
                {message.Id},
                {message.To.Value},
                {message.Subject.Value},
                {message.Body.Value},
                {message.CreatedAt},
                {message.UpdatedAt},
                {message.ExpireAt},
                {message.Attempts},
                {message.State},
                {message.SourceId}
            WHERE EXISTS (SELECT 1 FROM deduplicatiton)
            ON CONFLICT (source_id) DO NOTHING;
            """,
            ct);

        if (affected == 0)
        {
            _emailMetrics.DuplicateEmail();
        }
        else
        {
            _emailMetrics.EmailEnqueued();
        }

        return affected;
    }

    async Task<IReadOnlyList<EmailMessage>> IEmailQueueInternal.GetNextBatch(
        int batchSize,
        CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var retryFailureUpdatedAtThreshold = now - _emailQueueOptions.RetryCooldown;
        return await Set<EmailMessage>()
            .FromSql(
                $"""
                WITH next_batch AS (
                    SELECT id
                    FROM email.messages
                    WHERE (
                        expire_at is null
                        OR expire_at > {now}
                    )
                    AND (
                        state = {EmailMessageState.Pending}
                        OR (
                            state = {EmailMessageState.FailedRetryable}
                            AND updated_at <= {retryFailureUpdatedAtThreshold}
                            AND attempts < {_emailQueueOptions.MaxSendingAttempts}
                        )
                    )
                    ORDER BY created_at
                    LIMIT {batchSize}
                    FOR UPDATE
                    SKIP LOCKED
                ),
                sending AS (
                    UPDATE email.messages
                    SET
                        updated_at = {now},
                        state = {EmailMessageState.Sending},
                        attempts = attempts + 1
                    WHERE id in (SELECT id from next_batch)
                    RETURNING id, "to", subject, body, created_at, updated_at, expire_at, attempts, state, source_id, xmin
                )
                SELECT *
                FROM sending
                ORDER BY created_at
                """)
            .AsNoTracking()
            .ToArrayAsync(ct);
    }

    async Task IEmailQueueInternal.Acknowledge(
        Guid messageId,
        uint messageVersion,
        EmailMessageState state,
        DateTimeOffset at,
        CancellationToken ct)
    {
        if (!state.IsAcknowledgeable())
        {
            throw new InvalidOperationException(
                $"Email message cannot be acknowledged with state '{state}'.");
        }

        var updated = await Set<EmailMessage>()
            .Where(m => m.Id == messageId && m.Version == messageVersion)
            .ExecuteUpdateAsync(setters =>
            {
                setters.SetProperty(m => m.State, state);
                setters.SetProperty(m => m.UpdatedAt, at);
            }, ct);

        if (updated == 0)
        {
            throw new ConcurrencyConflictException("A concurrency conflict occurred while acknowledging EmailMessage.");
        }
    }

    async Task<EmailQueueReadyMetrics> IEmailQueueInternal.GetReadyMessagesMetrics(
        CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var retryFailureUpdatedAtThreshold = now - _emailQueueOptions.RetryCooldown;

        return await Database.SqlQuery<EmailQueueReadyMetrics>(
            $"""
            SELECT
                COUNT(*)::integer AS "TotalReady",
                MIN(created_at) AS "OldestReady"
            FROM email.messages
            WHERE
                (
                    expire_at IS NULL
                    OR expire_at > {now}
                )
                AND (
                    state = {EmailMessageState.Pending}
                    OR (
                        state = {EmailMessageState.FailedRetryable}
                        AND updated_at <= {retryFailureUpdatedAtThreshold}
                        AND attempts < {_emailQueueOptions.MaxSendingAttempts}
                    )
                )
            """)
            .SingleAsync(ct);
    }

    Task<int> IEmailQueueInternal.GetTotalMessagesCount(CancellationToken ct)
    {
        return Set<EmailMessage>().CountAsync(ct);
    }

    async Task IEmailQueueInternal.ArchiveMessagesInTerminationStates(int batchSize, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var sendingStaleUpdatedAtThreshold = now - _emailQueueOptions.SendingHold;

        await using var transaction = await Database.BeginTransactionAsync(ct);
        var messagesToArchive = await Set<EmailMessageHistory>()
            .FromSql(
                $"""
                WITH to_delete AS (
                    SELECT id
                    FROM email.messages
                    WHERE
                        (
                            expire_at IS NOT NULL
                            AND expire_at <= {now}
                            AND state <> {EmailMessageState.Sending}
                        )
                        OR state = {EmailMessageState.Sent}
                        OR state = {EmailMessageState.FailedPermanent}
                        OR (
                            state = {EmailMessageState.FailedRetryable}
                            AND attempts >= {_emailQueueOptions.MaxSendingAttempts}
                        )
                        OR (
                            state = {EmailMessageState.Sending}
                            AND updated_at <= {sendingStaleUpdatedAtThreshold}
                        )
                    ORDER BY created_at
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED
                ),
                deleted AS (
                    DELETE FROM email.messages mes
                    USING to_delete td
                    WHERE mes.id = td.id
                    RETURNING
                        mes.id as message_id,
                        mes."to",
                        mes.subject,
                        mes.body,
                        mes.created_at,
                        mes.updated_at,
                        mes.expire_at,
                        mes.attempts,
                        mes.state,
                        mes.source_id
                )
                SELECT
                    *,
                    CASE
                        WHEN state = {EmailMessageState.Sent}
                            THEN {EmailMessageArchiveReason.Sent}
                        WHEN state = {EmailMessageState.FailedPermanent}
                            THEN {EmailMessageArchiveReason.PermanentFailure}
                        WHEN
                            state = {EmailMessageState.FailedRetryable}
                            AND attempts >= {_emailQueueOptions.MaxSendingAttempts}
                            THEN {EmailMessageArchiveReason.AttemptsExhausted}
                        WHEN
                            state = {EmailMessageState.Sending}
                            AND updated_at <= {sendingStaleUpdatedAtThreshold}
                            THEN {EmailMessageArchiveReason.SendingStale}
                        ELSE {EmailMessageArchiveReason.Expired}
                    END AS archive_reason,
                    {now} AS archived_at
                FROM deleted
                """)
            .AsNoTracking()
            .ToListAsync(ct);

        if (messagesToArchive.Count == 0) return;

        Set<EmailMessageHistory>().AddRange(messagesToArchive);
        await SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var expiredCount = messagesToArchive.Count(m => m.ArchiveReason == EmailMessageArchiveReason.Expired);
        _emailMetrics.EmailsExpired(expiredCount);

        var attemptsExhaustedCount = messagesToArchive.Count(m => m.ArchiveReason == EmailMessageArchiveReason.AttemptsExhausted);
        _emailMetrics.EmailsAttemptsExhausted(attemptsExhaustedCount);

        var sendingStaleCount = messagesToArchive.Count(m => m.ArchiveReason == EmailMessageArchiveReason.SendingStale);
        _emailMetrics.EmailsSendingStale(sendingStaleCount);
    }

    async Task IEmailQueueInternal.ClearExpiredDeduplicationEntries(CancellationToken ct)
    {
        var createdAtThreshold = _timeProvider.GetUtcNow() - _emailQueueOptions.DeduplicationPeriod;
        await Database.ExecuteSqlAsync(
            $"""
            DELETE FROM email.messages_deduplication
            WHERE created_at <= {createdAtThreshold}
            """,
            ct);
    }
}
