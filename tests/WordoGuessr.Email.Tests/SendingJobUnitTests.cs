using System.Diagnostics.Metrics;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Infra.EmailSending;
using WordoGuessr.Email.Infra.EmailSending.BackgroundJobs;
using WordoGuessr.Email.Infra.EmailSending.Exceptions;
using WordoGuessr.Email.Domain;
using Xunit;

namespace WordoGuessr.Email.Tests;

public sealed class SendingJobUnitTests
{
    private static readonly DateTimeOffset _currentTime =
        new DateTimeOffset(2026, 8, 17, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenBatchIsEmpty_ShouldSendNoOp()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(10, ct))
            .Returns(Array.Empty<EmailMessage>());
        var job = CreateJob(emailSender);

        // ACT
        await job.Handle(emailQueue, batchSize: 10, ct);

        // ASSERT
        A.CallTo(() => emailSender.NoOp(ct)).MustHaveHappenedOnceExactly();
        A.CallTo(() => emailSender.SendEmail(
                A<EmailAddress>._,
                A<EmailSubject>._,
                A<EmailHtmlBody>._,
                A<Guid>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => emailQueue.Acknowledge(
                A<Guid>._,
                A<uint>._,
                A<EmailMessageState>._,
                A<DateTimeOffset>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenSendingSucceeds_ShouldSendAndAcknowledgeEveryMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var firstMessage = BuildMessage("first@wordoguessr.test", _currentTime);
        var secondMessage = BuildMessage("second@wordoguessr.test", _currentTime.AddSeconds(1));
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(2, ct))
            .Returns(new[] { firstMessage, secondMessage });
        var job = CreateJob(emailSender);

        // ACT
        await job.Handle(emailQueue, batchSize: 2, ct);

        // ASSERT
        AssertSent(emailSender, firstMessage, ct);
        AssertSent(emailSender, secondMessage, ct);
        AssertAcknowledged(emailQueue, firstMessage, EmailMessageState.Sent, ct);
        AssertAcknowledged(emailQueue, secondMessage, EmailMessageState.Sent, ct);
        A.CallTo(() => emailSender.NoOp(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenSendingFailsWithRetryableError_ShouldAcknowledgeRetryableFailure()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var message = BuildMessage("retryable@wordoguessr.test", _currentTime);
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(1, ct))
            .Returns(new[] { message });
        A.CallTo(() => emailSender.SendEmail(
                message.To,
                message.Subject,
                message.Body,
                message.SourceId,
                ct))
            .Returns(Task.FromException(
                new EmailSenderException(EmailSenderFailureState.Retryable)));
        var job = CreateJob(emailSender);

        // ACT
        await job.Handle(emailQueue, batchSize: 1, ct);

        // ASSERT
        A.CallTo(() => emailQueue.Acknowledge(
                message.Id,
                message.Version,
                EmailMessageState.FailedRetryable,
                _currentTime,
                ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenSendingFailsWithUnknownError_ShouldAcknowledgePermanentFailureAndContinue()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var failingMessage = BuildMessage("failing@wordoguessr.test", _currentTime);
        var succeedingMessage = BuildMessage("succeeding@wordoguessr.test", _currentTime.AddSeconds(1));
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(2, ct))
            .Returns(new[] { failingMessage, succeedingMessage });
        A.CallTo(() => emailSender.SendEmail(
                failingMessage.To,
                failingMessage.Subject,
                failingMessage.Body,
                failingMessage.SourceId,
                ct))
            .Returns(Task.FromException(
                new EmailSenderException(EmailSenderFailureState.Unknown)));
        var job = CreateJob(emailSender);

        // ACT
        await job.Handle(emailQueue, batchSize: 2, ct);

        // ASSERT
        AssertAcknowledged(
            emailQueue,
            failingMessage,
            EmailMessageState.FailedPermanent,
            ct);
        AssertSent(emailSender, succeedingMessage, ct);
        AssertAcknowledged(emailQueue, succeedingMessage, EmailMessageState.Sent, ct);
    }

    [Fact]
    public async Task Handle_WhenSendingIsCancelled_ShouldPropagateWithoutAcknowledging()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var firstMessage = BuildMessage("first@wordoguessr.test", _currentTime);
        var secondMessage = BuildMessage("second@wordoguessr.test", _currentTime.AddSeconds(1));
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(2, ct))
            .Returns(new[] { firstMessage, secondMessage });
        A.CallTo(() => emailSender.SendEmail(
                firstMessage.To,
                firstMessage.Subject,
                firstMessage.Body,
                firstMessage.SourceId,
                ct))
            .Returns(Task.FromException(new OperationCanceledException(ct)));
        var job = CreateJob(emailSender);

        // ACT
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => job.Handle(emailQueue, batchSize: 2, ct));

        // ASSERT
        A.CallTo(() => emailQueue.Acknowledge(
                A<Guid>._,
                A<uint>._,
                A<EmailMessageState>._,
                A<DateTimeOffset>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => emailSender.SendEmail(
                secondMessage.To,
                secondMessage.Subject,
                secondMessage.Body,
                secondMessage.SourceId,
                A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenAcknowledgeFails_ShouldContinueAndThrowAggregateException()
    {
        var ct = TestContext.Current.CancellationToken;

        // ARRANGE
        var firstMessage = BuildMessage("first@wordoguessr.test", _currentTime);
        var secondMessage = BuildMessage("second@wordoguessr.test", _currentTime.AddSeconds(1));
        var acknowledgeException = new InvalidOperationException("Acknowledge failed");
        var emailQueue = A.Fake<IEmailQueueInternal>();
        var emailSender = A.Fake<IEmailSender>();
        A.CallTo(() => emailQueue.GetNextBatch(2, ct))
            .Returns(new[] { firstMessage, secondMessage });
        A.CallTo(() => emailQueue.Acknowledge(
                firstMessage.Id,
                firstMessage.Version,
                EmailMessageState.Sent,
                _currentTime,
                ct))
            .Returns(Task.FromException(acknowledgeException));
        var job = CreateJob(emailSender);

        // ACT
        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => job.Handle(emailQueue, batchSize: 2, ct));

        // ASSERT
        exception.InnerExceptions.ShouldHaveSingleItem().ShouldBeSameAs(acknowledgeException);
        A.CallTo(() => emailSender.SendEmail(
                A<EmailAddress>._,
                A<EmailSubject>._,
                A<EmailHtmlBody>._,
                A<Guid>._,
                ct))
            .MustHaveHappenedANumberOfTimesMatching(callCount => callCount == 2);
        A.CallTo(() => emailQueue.Acknowledge(
                secondMessage.Id,
                secondMessage.Version,
                EmailMessageState.Sent,
                _currentTime,
                ct))
            .MustHaveHappenedOnceExactly();
    }

    private static SendingJob CreateJob(IEmailSender emailSender)
    {
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetUtcNow(_currentTime);

        return new SendingJob(
            A.Fake<IServiceScopeFactory>(),
            emailSender,
            timeProvider,
            NullLogger<SendingJob>.Instance,
            new EmailMetrics(CreateMeterFactory()),
            Options.Create(new SendingJobOptions
            {
                Interval = TimeSpan.FromSeconds(5),
                BatchSize = 20
            }));
    }

    private static IMeterFactory CreateMeterFactory()
    {
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => meterFactory.Create(A<MeterOptions>._))
            .Returns(new Meter($"{nameof(SendingJobUnitTests)}.{Guid.NewGuid()}"));
        return meterFactory;
    }

    private static void AssertSent(
        IEmailSender emailSender,
        EmailMessage message,
        CancellationToken ct)
    {
        A.CallTo(() => emailSender.SendEmail(
                message.To,
                message.Subject,
                message.Body,
                message.SourceId,
                ct))
            .MustHaveHappenedOnceExactly();
    }

    private static void AssertAcknowledged(
        IEmailQueueInternal emailQueue,
        EmailMessage message,
        EmailMessageState state,
        CancellationToken ct)
    {
        A.CallTo(() => emailQueue.Acknowledge(
                message.Id,
                message.Version,
                state,
                _currentTime,
                ct))
            .MustHaveHappenedOnceExactly();
    }

    private static EmailMessage BuildMessage(string recipient, DateTimeOffset createdAt)
    {
        return new EmailMessage(
            EmailAddress.Create(recipient),
            EmailSubject.Create($"Subject for {recipient}"),
            EmailHtmlBody.Create($"<p>{recipient}</p>"),
            createdAt,
            Guid.CreateVersion7(createdAt))
        {
            Attempts = 1,
            State = EmailMessageState.Sending,
            UpdatedAt = _currentTime
        };
    }
}
