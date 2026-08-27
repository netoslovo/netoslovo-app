using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Domain;
using WordoGuessr.Email.Infra.Database;
using WordoGuessr.Email.Infra.EmailSending;
using WordoGuessr.Testing.Common;
using Xunit;

namespace WordoGuessr.Email.Tests;

public sealed class EmailQueueTests : IClassFixture<EmailDbContextFixture>
{
    private static readonly DateTimeOffset _currentTime =
        new DateTimeOffset(2026, 8, 17, 10, 0, 0, TimeSpan.Zero);

    private readonly EmailDbContextOptions _dbContextOptions =
        new EmailDbContextOptions { EnableDebugLogging = true };
    private readonly EmailQueueOptions _emailQueueOptions = new EmailQueueOptions
    {
        MaxSendingAttempts = 3,
        SendingHold = TimeSpan.FromMinutes(5),
        RetryCooldown = TimeSpan.FromSeconds(30),
        DeduplicationPeriod = TimeSpan.FromDays(1)
    };
    private readonly FakeTimeProvider _timeProvider;
    private readonly EmailDbContextFixture _fixture;
    private readonly ITestOutputHelper _output;

    public EmailQueueTests(EmailDbContextFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(_currentTime);
    }

    [Fact]
    public async Task GetNextBatch_ShouldReserveEarliestEligibleMessagesUpToBatchSize()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);

        var baseTime = _currentTime.AddMinutes(-10);
        var activeTtl = TimeSpan.FromDays(365);

        var recentSendingMessage = BuildSeededMessage(
            "recent-sending@wordoguessr.test",
            EmailMessageState.Sending,
            baseTime.AddMinutes(-2),
            attempts: 0,
            ttl: activeTtl,
            updated: _currentTime.AddMinutes(-1));

        var sentMessage = BuildSeededMessage(
            "sent@wordoguessr.test",
            EmailMessageState.Sent,
            baseTime.AddMinutes(-1),
            attempts: 0,
            ttl: activeTtl);

        var firstEligible = BuildSeededMessage(
            "first@wordoguessr.test",
            EmailMessageState.Pending,
            baseTime,
            attempts: 0,
            ttl: activeTtl);

        var secondEligible = BuildSeededMessage(
            "second@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            baseTime.AddMinutes(1),
            attempts: 1,
            ttl: activeTtl);

        var staleSendingMessage = BuildSeededMessage(
            "stale-sending@wordoguessr.test",
            EmailMessageState.Sending,
            baseTime.AddMinutes(2),
            attempts: 0,
            ttl: activeTtl,
            updated: _currentTime.Subtract(_emailQueueOptions.SendingHold).AddMinutes(-1));

        var exhaustedFailed = BuildSeededMessage(
            "exhausted-failed@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            baseTime.AddMinutes(3),
            attempts: _emailQueueOptions.MaxSendingAttempts,
            ttl: activeTtl);

        var thirdEligible = BuildSeededMessage(
            "third@wordoguessr.test",
            EmailMessageState.Pending,
            baseTime.AddMinutes(4),
            attempts: 0,
            ttl: activeTtl);

        await SeedMessageAsync(arrangeDbContext, recentSendingMessage, ct);
        await SeedMessageAsync(arrangeDbContext, sentMessage, ct);
        await SeedMessageAsync(arrangeDbContext, firstEligible, ct);
        await SeedMessageAsync(arrangeDbContext, secondEligible, ct);
        await SeedMessageAsync(arrangeDbContext, staleSendingMessage, ct);
        await SeedMessageAsync(arrangeDbContext, exhaustedFailed, ct);
        await SeedMessageAsync(arrangeDbContext, thirdEligible, ct);

        // ACT
        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var batch = await ((IEmailQueueInternal)actDbContext).GetNextBatch(3, ct);

        // ASSERT
        batch.Select(message => message.To.Value).ShouldBe(new[]
        {
            firstEligible.To.Value,
            secondEligible.To.Value,
            thirdEligible.To.Value
        });
        batch.ShouldAllBe(message => message.State == EmailMessageState.Sending);

        var selectedById = batch.ToDictionary(message => message.Id);
        selectedById[firstEligible.Id].Attempts.ShouldBe(1);
        selectedById[secondEligible.Id].Attempts.ShouldBe(2);
        selectedById[thirdEligible.Id].Attempts.ShouldBe(1);
        selectedById.Values.ShouldAllBe(
            message => message.UpdatedAt == _currentTime);

        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var messages = await LoadMessagesByIdAsync(assertDbContext, ct);

        AssertReserved(messages[firstEligible.Id], attempts: 1);
        AssertReserved(messages[secondEligible.Id], attempts: 2);
        AssertReserved(messages[thirdEligible.Id], attempts: 1);
        AssertUnchanged(messages[staleSendingMessage.Id], staleSendingMessage);
        AssertUnchanged(messages[exhaustedFailed.Id], exhaustedFailed);
        AssertUnchanged(messages[recentSendingMessage.Id], recentSendingMessage);
        AssertUnchanged(messages[sentMessage.Id], sentMessage);
    }

    [Fact]
    public async Task GetNextBatch_ShouldSkipExpiredAndCoolingDownMessages()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);

        var expiredTtl = TimeSpan.FromMinutes(5);
        var activeTtl = TimeSpan.FromMinutes(30);

        var expiredPending = BuildSeededMessage(
            "expired-pending@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.Subtract(expiredTtl).AddMinutes(-1),
            attempts: 1,
            ttl: expiredTtl);

        var expiredFailed = BuildSeededMessage(
            "expired-failed@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            _currentTime.Subtract(expiredTtl).AddMinutes(-2),
            attempts: 1,
            ttl: expiredTtl);

        var freshSending = BuildSeededMessage(
            "fresh-sending@wordoguessr.test",
            EmailMessageState.Sending,
            _currentTime.AddMinutes(-2),
            attempts: 1,
            ttl: activeTtl,
            updated: _currentTime.AddMinutes(-1));

        var coolingDownFailure = BuildSeededMessage(
            "cooling-down@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            _currentTime.AddMinutes(-2),
            attempts: 1,
            ttl: activeTtl,
            updated: _currentTime.AddSeconds(-10));

        var freshPending = BuildSeededMessage(
            "fresh@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.Subtract(activeTtl).AddMinutes(1),
            attempts: 0,
            ttl: activeTtl);

        await SeedMessageAsync(arrangeDbContext, expiredPending, ct);
        await SeedMessageAsync(arrangeDbContext, expiredFailed, ct);
        await SeedMessageAsync(arrangeDbContext, freshSending, ct);
        await SeedMessageAsync(arrangeDbContext, coolingDownFailure, ct);
        await SeedMessageAsync(arrangeDbContext, freshPending, ct);

        // ACT
        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var batch = await ((IEmailQueueInternal)actDbContext).GetNextBatch(10, ct);

        // ASSERT
        batch.ShouldHaveSingleItem().Id.ShouldBe(freshPending.Id);

        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var messages = await LoadMessagesByIdAsync(assertDbContext, ct);

        AssertUnchanged(messages[expiredPending.Id], expiredPending);
        AssertUnchanged(messages[expiredFailed.Id], expiredFailed);
        AssertUnchanged(messages[freshSending.Id], freshSending);
        AssertUnchanged(messages[coolingDownFailure.Id], coolingDownFailure);
        AssertReserved(messages[freshPending.Id], attempts: 1);
    }

    [Fact]
    public async Task Acknowledge_ShouldPersistRequestedStates()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);

        var sentMessage = BuildSeededMessage(
            "sent@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-2),
            attempts: 0);
        var failedMessage = BuildSeededMessage(
            "failed@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-1),
            attempts: 0);
        await SeedMessageAsync(arrangeDbContext, sentMessage, ct);
        await SeedMessageAsync(arrangeDbContext, failedMessage, ct);

        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var emailQueue = (IEmailQueueInternal)actDbContext;
        var batch = await emailQueue.GetNextBatch(2, ct);
        var reservedById = batch.ToDictionary(message => message.Id);
        var acknowledgedAt = _currentTime.AddSeconds(1);

        // ACT
        await emailQueue.Acknowledge(
            sentMessage.Id,
            reservedById[sentMessage.Id].Version,
            EmailMessageState.Sent,
            acknowledgedAt,
            ct);
        await emailQueue.Acknowledge(
            failedMessage.Id,
            reservedById[failedMessage.Id].Version,
            EmailMessageState.FailedPermanent,
            acknowledgedAt,
            ct);

        // ASSERT
        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var messages = await LoadMessagesByIdAsync(assertDbContext, ct);
        messages[sentMessage.Id].State.ShouldBe(EmailMessageState.Sent);
        messages[sentMessage.Id].UpdatedAt.ShouldBeAtPostgreSqlPrecision(acknowledgedAt);
        messages[failedMessage.Id].State.ShouldBe(EmailMessageState.FailedPermanent);
        messages[failedMessage.Id].UpdatedAt.ShouldBeAtPostgreSqlPrecision(acknowledgedAt);
    }

    [Fact]
    public async Task EnqueueMessage_WithDuplicateSourceId_ShouldKeepFirstMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(actDbContext, ct);
        var sourceId = Guid.NewGuid();
        var firstMessage = BuildMessage("first@wordoguessr.test", sourceId);
        var duplicateMessage = BuildMessage("duplicate@wordoguessr.test", sourceId);

        // ACT
        await actDbContext.EnqueueMessage(firstMessage, ct);
        await actDbContext.EnqueueMessage(duplicateMessage, ct);

        // ASSERT
        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var persistedMessages = await assertDbContext.Set<EmailMessage>()
            .AsNoTracking()
            .ToArrayAsync(ct);
        var persistedMessage = persistedMessages.ShouldHaveSingleItem();
        persistedMessage.Id.ShouldBe(firstMessage.Id);
        persistedMessage.To.Value.ShouldBe(firstMessage.To.Value);
    }

    [Fact]
    public async Task GetReadyMessagesMetrics_ShouldReturnTotalAndOldestReadyMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var dbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(dbContext, ct);

        var oldestReady = BuildSeededMessage(
            "oldest-ready@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-10),
            attempts: 0);
        var newestReady = BuildSeededMessage(
            "newest-ready@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-5),
            attempts: 0);
        var sent = BuildSeededMessage(
            "sent@wordoguessr.test",
            EmailMessageState.Sent,
            _currentTime.AddMinutes(-20),
            attempts: 1);

        await SeedMessageAsync(dbContext, newestReady, ct);
        await SeedMessageAsync(dbContext, sent, ct);
        await SeedMessageAsync(dbContext, oldestReady, ct);

        // ACT
        var metrics = await ((IEmailQueueInternal)dbContext).GetReadyMessagesMetrics(ct);

        // ASSERT
        metrics.TotalReady.ShouldBe(2);
        metrics.OldestReady.ShouldNotBeNull()
            .ShouldBeAtPostgreSqlPrecision(oldestReady.Created);
    }

    [Fact]
    public async Task GetReadyMessagesMetrics_WhenQueueIsEmpty_ShouldReturnZeroAndNoOldestMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var dbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(dbContext, ct);

        // ACT
        var metrics = await ((IEmailQueueInternal)dbContext).GetReadyMessagesMetrics(ct);

        // ASSERT
        metrics.TotalReady.ShouldBe(0);
        metrics.OldestReady.ShouldBeNull();
    }

    [Fact]
    public async Task ArchiveMessagesInTerminationStates_ShouldMoveEligibleMessagesToHistory()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);

        var sent = BuildSeededMessage(
            "sent@wordoguessr.test",
            EmailMessageState.Sent,
            _currentTime.AddMinutes(-8),
            attempts: 1);
        var expired = BuildSeededMessage(
            "expired@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-7),
            attempts: 0,
            ttl: TimeSpan.FromMinutes(7));
        var attemptsExhausted = BuildSeededMessage(
            "attempts-exhausted@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            _currentTime.AddMinutes(-6),
            attempts: _emailQueueOptions.MaxSendingAttempts);
        var sendingStale = BuildSeededMessage(
            "sending-stale@wordoguessr.test",
            EmailMessageState.Sending,
            _currentTime.AddMinutes(-5),
            attempts: 1,
            updated: _currentTime - _emailQueueOptions.SendingHold);
        var permanentFailure = BuildSeededMessage(
            "permanent-failure@wordoguessr.test",
            EmailMessageState.FailedPermanent,
            _currentTime.AddMinutes(-4),
            attempts: 1);
        var expiredSending = BuildSeededMessage(
            "expired-sending@wordoguessr.test",
            EmailMessageState.Sending,
            _currentTime.AddMinutes(-4),
            attempts: 1,
            ttl: TimeSpan.FromMinutes(3),
            updated: _currentTime - _emailQueueOptions.SendingHold + TimeSpan.FromSeconds(1));
        var pending = BuildSeededMessage(
            "pending@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-4),
            attempts: 0);
        var retryable = BuildSeededMessage(
            "retryable@wordoguessr.test",
            EmailMessageState.FailedRetryable,
            _currentTime.AddMinutes(-3),
            attempts: _emailQueueOptions.MaxSendingAttempts - 1);
        var sending = BuildSeededMessage(
            "sending@wordoguessr.test",
            EmailMessageState.Sending,
            _currentTime.AddMinutes(-2),
            attempts: 1,
            updated: _currentTime - _emailQueueOptions.SendingHold + TimeSpan.FromSeconds(1));

        await SeedMessageAsync(arrangeDbContext, sent, ct);
        await SeedMessageAsync(arrangeDbContext, expired, ct);
        await SeedMessageAsync(arrangeDbContext, attemptsExhausted, ct);
        await SeedMessageAsync(arrangeDbContext, sendingStale, ct);
        await SeedMessageAsync(arrangeDbContext, permanentFailure, ct);
        await SeedMessageAsync(arrangeDbContext, expiredSending, ct);
        await SeedMessageAsync(arrangeDbContext, pending, ct);
        await SeedMessageAsync(arrangeDbContext, retryable, ct);
        await SeedMessageAsync(arrangeDbContext, sending, ct);

        // ACT
        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ((IEmailQueueInternal)actDbContext).ArchiveMessagesInTerminationStates(10, ct);

        // ASSERT
        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var messages = await LoadMessagesByIdAsync(assertDbContext, ct);
        messages.Keys.ShouldBe(
            new[] { expiredSending.Id, pending.Id, retryable.Id, sending.Id },
            ignoreOrder: true);

        var history = await LoadHistoryByIdAsync(assertDbContext, ct);
        history.Keys.ShouldBe(
            new[]
            {
                sent.Id,
                expired.Id,
                attemptsExhausted.Id,
                sendingStale.Id,
                permanentFailure.Id
            },
            ignoreOrder: true);
        AssertArchived(history[sent.Id], sent, EmailMessageArchiveReason.Sent);
        AssertArchived(history[expired.Id], expired, EmailMessageArchiveReason.Expired);
        AssertArchived(
            history[attemptsExhausted.Id],
            attemptsExhausted,
            EmailMessageArchiveReason.AttemptsExhausted);
        AssertArchived(history[sendingStale.Id], sendingStale, EmailMessageArchiveReason.SendingStale);
        AssertArchived(
            history[permanentFailure.Id],
            permanentFailure,
            EmailMessageArchiveReason.PermanentFailure);
    }

    [Fact]
    public async Task ArchiveMessagesInTerminationStates_ShouldArchiveOldestMessagesUpToBatchSize()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);

        var oldest = BuildSeededMessage(
            "oldest@wordoguessr.test",
            EmailMessageState.Sent,
            _currentTime.AddMinutes(-3),
            attempts: 1);
        var second = BuildSeededMessage(
            "second@wordoguessr.test",
            EmailMessageState.Sent,
            _currentTime.AddMinutes(-2),
            attempts: 1);
        var newest = BuildSeededMessage(
            "newest@wordoguessr.test",
            EmailMessageState.Sent,
            _currentTime.AddMinutes(-1),
            attempts: 1);

        await SeedMessageAsync(arrangeDbContext, newest, ct);
        await SeedMessageAsync(arrangeDbContext, oldest, ct);
        await SeedMessageAsync(arrangeDbContext, second, ct);

        // ACT
        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ((IEmailQueueInternal)actDbContext).ArchiveMessagesInTerminationStates(2, ct);

        // ASSERT
        await using var assertDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var remaining = await LoadMessagesByIdAsync(assertDbContext, ct);
        remaining.Keys.ShouldHaveSingleItem().ShouldBe(newest.Id);

        var history = await LoadHistoryByIdAsync(assertDbContext, ct);
        history.Keys.ShouldBe(new[] { oldest.Id, second.Id }, ignoreOrder: true);
    }

    [Fact]
    public async Task ClearExpiredDeduplicationEntries_ShouldDeleteOnlyExpiredEntries()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var dbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(dbContext, ct);

        var expiredSourceId = Guid.NewGuid();
        var activeSourceId = Guid.NewGuid();
        var expirationThreshold = _currentTime - _emailQueueOptions.DeduplicationPeriod;
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO email.messages_deduplication (source_id, created_at)
            VALUES
                ({expiredSourceId}, {expirationThreshold}),
                ({activeSourceId}, {expirationThreshold.AddSeconds(1)})
            """,
            ct);

        // ACT
        await ((IEmailQueueInternal)dbContext).ClearExpiredDeduplicationEntries(ct);

        // ASSERT
        var remainingSourceIds = await dbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT source_id AS "Value"
                FROM email.messages_deduplication
                """)
            .ToArrayAsync(ct);
        remainingSourceIds.ShouldHaveSingleItem().ShouldBe(activeSourceId);
    }

    [Fact]
    public async Task Acknowledge_WithStaleVersion_ShouldThrowConcurrencyConflict()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        await using var arrangeDbContext = _fixture.CreateDbContext(
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        await ClearMessagesAsync(arrangeDbContext, ct);
        var seededMessage = BuildSeededMessage(
            "recipient@wordoguessr.test",
            EmailMessageState.Pending,
            _currentTime.AddMinutes(-1),
            attempts: 0);
        await SeedMessageAsync(arrangeDbContext, seededMessage, ct);

        await using var actDbContext = _fixture.CreateDbContext(
            _output,
            _dbContextOptions,
            _emailQueueOptions,
            _timeProvider);
        var emailQueue = (IEmailQueueInternal)actDbContext;
        var reservedMessage = (await emailQueue.GetNextBatch(1, ct)).ShouldHaveSingleItem();

        // ACT
        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => emailQueue.Acknowledge(
                reservedMessage.Id,
                reservedMessage.Version + 1,
                EmailMessageState.Sent,
                _currentTime,
                ct));

        // ASSERT
        exception.Message.ShouldNotBeNullOrWhiteSpace();
    }

    private static async Task ClearMessagesAsync(EmailDbContext dbContext, CancellationToken ct)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE email.messages, email.messages_history, email.messages_deduplication
            """,
            ct);
    }

    private static async Task SeedMessageAsync(
        EmailDbContext dbContext,
        SeededMessage message,
        CancellationToken ct)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO email.messages (
                id,
                "to",
                subject,
                body,
                created_at,
                expire_at,
                attempts,
                state,
                updated_at,
                source_id)
            VALUES (
                {message.Id},
                {message.To.Value},
                {message.Subject.Value},
                {message.Body.Value},
                {message.Created},
                {(message.Ttl is null ? null : message.Created + message.Ttl.Value)},
                {message.Attempts},
                {(int)message.State},
                {message.Updated},
                {message.SourceId})
            """,
            ct);
    }

    private static async Task<Dictionary<Guid, EmailMessage>> LoadMessagesByIdAsync(
        EmailDbContext dbContext,
        CancellationToken ct)
    {
        return await dbContext.Set<EmailMessage>()
            .AsNoTracking()
            .ToDictionaryAsync(message => message.Id, ct);
    }

    private static async Task<Dictionary<Guid, EmailMessageHistory>> LoadHistoryByIdAsync(
        EmailDbContext dbContext,
        CancellationToken ct)
    {
        return await dbContext.Set<EmailMessageHistory>()
            .AsNoTracking()
            .ToDictionaryAsync(message => message.MessageId, ct);
    }

    private static SeededMessage BuildSeededMessage(
        string recipient,
        EmailMessageState state,
        DateTimeOffset created,
        int attempts,
        TimeSpan? ttl = null,
        DateTimeOffset? updated = null)
    {
        return new SeededMessage(
            Guid.CreateVersion7(created),
            EmailAddress.Create(recipient),
            EmailSubject.Create($"Subject for {recipient}"),
            EmailHtmlBody.Create($"<p>{recipient}</p>"),
            created,
            ttl,
            attempts,
            state,
            updated ?? created,
            Guid.CreateVersion7(created));
    }

    private static EmailMessage BuildMessage(string recipient, Guid sourceId) =>
        new EmailMessage(
            EmailAddress.Create(recipient),
            EmailSubject.Create($"Subject for {recipient}"),
            EmailHtmlBody.Create($"<p>{recipient}</p>"),
            _currentTime,
            sourceId);

    private void AssertReserved(EmailMessage message, int attempts)
    {
        message.State.ShouldBe(EmailMessageState.Sending);
        message.Attempts.ShouldBe(attempts);
        message.UpdatedAt.ShouldBeAtPostgreSqlPrecision(_currentTime);
    }

    private static void AssertUnchanged(EmailMessage actual, SeededMessage expected)
    {
        actual.State.ShouldBe(expected.State);
        actual.Attempts.ShouldBe(expected.Attempts);
        actual.UpdatedAt.ShouldBeAtPostgreSqlPrecision(expected.Updated);
    }

    private static void AssertArchived(
        EmailMessageHistory actual,
        SeededMessage expected,
        EmailMessageArchiveReason expectedReason)
    {
        actual.MessageId.ShouldBe(expected.Id);
        actual.To.ShouldBe(expected.To);
        actual.Subject.ShouldBe(expected.Subject);
        actual.CreatedAt.ShouldBeAtPostgreSqlPrecision(expected.Created);
        if (expected.Ttl is null)
        {
            actual.ExpireAt.ShouldBeNull();
        }
        else
        {
            actual.ExpireAt.ShouldNotBeNull()
                .ShouldBeAtPostgreSqlPrecision(expected.Created + expected.Ttl.Value);
        }
        actual.Attempts.ShouldBe(expected.Attempts);
        actual.State.ShouldBe(expected.State);
        actual.UpdatedAt.ShouldBeAtPostgreSqlPrecision(expected.Updated);
        actual.SourceId.ShouldBe(expected.SourceId);
        actual.ArchivedAt.ShouldBeAtPostgreSqlPrecision(_currentTime);
        actual.ArchiveReason.ShouldBe(expectedReason);
    }

    private sealed record SeededMessage(
        Guid Id,
        EmailAddress To,
        EmailSubject Subject,
        EmailHtmlBody Body,
        DateTimeOffset Created,
        TimeSpan? Ttl,
        int Attempts,
        EmailMessageState State,
        DateTimeOffset Updated,
        Guid SourceId);
}
