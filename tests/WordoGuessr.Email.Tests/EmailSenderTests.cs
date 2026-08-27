using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using FakeItEasy;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Shouldly;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Infra.EmailSending;
using WordoGuessr.Email.Infra.EmailSending.Exceptions;
using Xunit;

namespace WordoGuessr.Email.Tests;

public sealed class EmailSenderTests
{
    [Fact]
    public async Task SendEmail_ShouldConnectSendExpectedMessageAndReuseClient()
    {
        // ARRANGE
        var options = CreateOptions();
        var client = CreateClient();
        var factory = CreateFactory(client);
        var sentMessages = new List<MimeMessage>();
        A.CallTo(() => client.SendAsync(
                A<MimeMessage>._,
                A<CancellationToken>._,
                A<ITransferProgress?>._))
            .Invokes(call => sentMessages.Add(call.GetArgument<MimeMessage>(0)!))
            .Returns(Task.FromResult("OK"));
        using var sender = CreateSender(options, factory);

        var firstMessageId = Guid.NewGuid();
        var secondMessageId = Guid.NewGuid();

        // ACT
        await sender.SendEmail(
            EmailAddress.Create("first@wordoguessr.test"),
            EmailSubject.Create("First subject"),
            EmailHtmlBody.Create("<p>First body</p>"),
            firstMessageId,
            TestContext.Current.CancellationToken);
        await sender.SendEmail(
            EmailAddress.Create("second@wordoguessr.test"),
            EmailSubject.Create("Second subject"),
            EmailHtmlBody.Create("<p>Second body</p>"),
            secondMessageId,
            TestContext.Current.CancellationToken);

        // ASSERT
        A.CallTo(() => factory.Create()).MustHaveHappenedOnceExactly();
        A.CallTo(() => client.ConnectAsync(
                options.SmtpServer,
                options.SmtpPort,
                options.UseSSL,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        sentMessages.Count.ShouldBe(2);
        AssertMessage(
            sentMessages[0],
            "first@wordoguessr.test",
            "First subject",
            "<p>First body</p>",
            $"{firstMessageId}@{options.FromDomain}");
        AssertMessage(
            sentMessages[1],
            "second@wordoguessr.test",
            "Second subject",
            "<p>Second body</p>",
            $"{secondMessageId}@{options.FromDomain}");
    }

    [Fact]
    public async Task SendEmail_WhenCalledConcurrently_ShouldSerializeSending()
    {
        // ARRANGE
        var options = CreateOptions();
        var client = CreateClient();
        var factory = CreateFactory(client);
        var firstSendStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstSend = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sendCount = 0;
        var activeSendCount = 0;
        var maximumActiveSendCount = 0;
        A.CallTo(() => client.SendAsync(
                A<MimeMessage>._,
                A<CancellationToken>._,
                A<ITransferProgress?>._))
            .ReturnsLazily(async _ =>
            {
                var currentSend = Interlocked.Increment(ref sendCount);
                var activeSends = Interlocked.Increment(ref activeSendCount);
                maximumActiveSendCount = Math.Max(maximumActiveSendCount, activeSends);

                if (currentSend == 1)
                {
                    firstSendStarted.TrySetResult();
                    await releaseFirstSend.Task;
                }

                Interlocked.Decrement(ref activeSendCount);
                return "OK";
            });
        using var sender = CreateSender(options, factory);

        // ACT
        var firstSend = SendTestMessage(sender, "first@wordoguessr.test");
        await firstSendStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        var secondSend = SendTestMessage(sender, "second@wordoguessr.test");
        await Task.Yield();
        releaseFirstSend.TrySetResult();
        await Task.WhenAll(firstSend, secondSend);

        // ASSERT
        sendCount.ShouldBe(2);
        maximumActiveSendCount.ShouldBe(1);
    }

    [Fact]
    public async Task SendEmail_WhenConnectionTimesOut_ShouldThrowRetryableAndDisposeClient()
    {
        // ARRANGE
        var options = CreateOptions(TimeSpan.FromMilliseconds(100));
        var client = CreateClient();
        A.CallTo(() => client.ConnectAsync(
                A<string>._,
                A<int>._,
                A<bool>._,
                A<CancellationToken>._))
            .ReturnsLazily(call => Task.Delay(
                Timeout.InfiniteTimeSpan,
                call.GetArgument<CancellationToken>(3)));
        using var sender = CreateSender(options, CreateFactory(client));

        // ACT
        var exception = await Assert.ThrowsAsync<EmailSenderTimeoutException>(
            () => SendTestMessage(sender, "recipient@wordoguessr.test"));

        // ASSERT
        exception.FailureState.ShouldBe(EmailSenderFailureState.Retryable);
        A.CallTo(() => client.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SendEmail_WhenSendingTimesOut_ShouldThrowPermanentAndDisposeClient()
    {
        // ARRANGE
        var options = CreateOptions(TimeSpan.FromMilliseconds(100));
        var client = CreateClient();
        A.CallTo(() => client.SendAsync(
                A<MimeMessage>._,
                A<CancellationToken>._,
                A<ITransferProgress?>._))
            .ReturnsLazily(async call =>
            {
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    call.GetArgument<CancellationToken>(1));
                return "OK";
            });
        using var sender = CreateSender(options, CreateFactory(client));

        // ACT
        var exception = await Assert.ThrowsAsync<EmailSenderTimeoutException>(
            () => SendTestMessage(sender, "recipient@wordoguessr.test"));

        // ASSERT
        exception.FailureState.ShouldBe(EmailSenderFailureState.Permanent);
        A.CallTo(() => client.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task NoOp_WithoutInitializedClient_ShouldNotCreateClient()
    {
        // ARRANGE
        var factory = A.Fake<ISmtpClientFactory>();
        using var sender = CreateSender(CreateOptions(), factory);

        // ACT
        await sender.NoOp(TestContext.Current.CancellationToken);

        // ASSERT
        A.CallTo(() => factory.Create()).MustNotHaveHappened();
    }

    [Fact]
    public async Task NoOp_WhenCommandFails_ShouldInvalidateClientAndNotThrow()
    {
        // ARRANGE
        var failedClient = CreateClient();
        var replacementClient = CreateClient();
        A.CallTo(() => failedClient.NoOpAsync(A<CancellationToken>._))
            .Returns(Task.FromException(new IOException("NOOP failed")));
        var factory = CreateFactory(failedClient, replacementClient);
        using var sender = CreateSender(CreateOptions(), factory);
        await SendTestMessage(sender, "first@wordoguessr.test");

        // ACT
        await sender.NoOp(TestContext.Current.CancellationToken);
        await SendTestMessage(sender, "second@wordoguessr.test");

        // ASSERT
        A.CallTo(() => failedClient.Dispose()).MustHaveHappenedOnceExactly();
        A.CallTo(() => replacementClient.SendAsync(
                A<MimeMessage>._,
                A<CancellationToken>._,
                A<ITransferProgress?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task NoOp_WhenCommandTimesOut_ShouldThrowRetryableAndDisposeClient()
    {
        // ARRANGE
        var options = CreateOptions(TimeSpan.FromMilliseconds(100));
        var client = CreateClient();
        A.CallTo(() => client.NoOpAsync(A<CancellationToken>._))
            .ReturnsLazily(call => Task.Delay(
                Timeout.InfiniteTimeSpan,
                call.GetArgument<CancellationToken>(0)));
        using var sender = CreateSender(options, CreateFactory(client));
        await SendTestMessage(sender, "recipient@wordoguessr.test");

        // ACT
        var exception = await Assert.ThrowsAsync<EmailSenderTimeoutException>(
            () => sender.NoOp(TestContext.Current.CancellationToken));

        // ASSERT
        exception.FailureState.ShouldBe(EmailSenderFailureState.Retryable);
        A.CallTo(() => client.Dispose()).MustHaveHappenedOnceExactly();
    }

    private static EmailSenderOptions CreateOptions(TimeSpan? sendingTimeout = null) =>
        new EmailSenderOptions
        {
            SmtpServer = "smtp.wordoguessr.test",
            SmtpPort = 2525,
            FromBaseAddress = "no-reply",
            FromDomain = "wordoguessr.test",
            FromDisplayName = "WordoGuessr",
            SendingTimeout = sendingTimeout ?? TimeSpan.FromSeconds(5),
            UseSSL = false
        };

    private static ISmtpClient CreateClient()
    {
        var client = A.Fake<ISmtpClient>();
        A.CallTo(() => client.IsConnected).Returns(true);
        A.CallTo(() => client.ConnectAsync(
                A<string>._,
                A<int>._,
                A<bool>._,
                A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => client.SendAsync(
                A<MimeMessage>._,
                A<CancellationToken>._,
                A<ITransferProgress?>._))
            .Returns(Task.FromResult("OK"));
        A.CallTo(() => client.NoOpAsync(A<CancellationToken>._))
            .Returns(Task.CompletedTask);
        return client;
    }

    private static ISmtpClientFactory CreateFactory(params ISmtpClient[] clients)
    {
        var factory = A.Fake<ISmtpClientFactory>();
        A.CallTo(() => factory.Create()).ReturnsNextFromSequence(clients);
        return factory;
    }

    private static EmailSender CreateSender(EmailSenderOptions options, ISmtpClientFactory factory) =>
        new EmailSender(
            Options.Create(options),
            factory,
            NullLogger<EmailSender>.Instance,
            new SmtpMetrics(CreateMeterFactory()));

    private static IMeterFactory CreateMeterFactory()
    {
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => meterFactory.Create(A<MeterOptions>._))
            .Returns(new Meter($"{nameof(EmailSenderTests)}.{Guid.NewGuid()}"));
        return meterFactory;
    }

    private static Task SendTestMessage(EmailSender sender, string recipient) =>
        sender.SendEmail(
            EmailAddress.Create(recipient),
            EmailSubject.Create("Test subject"),
            EmailHtmlBody.Create("<p>Test body</p>"),
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

    private static void AssertMessage(
        MimeMessage message,
        string recipient,
        string subject,
        string htmlBody,
        string messageId)
    {
        message.From.Mailboxes.Single().Address.ShouldBe("no-reply@wordoguessr.test");
        message.From.Mailboxes.Single().Name.ShouldBe("WordoGuessr");
        message.To.Mailboxes.Single().Address.ShouldBe(recipient);
        message.Subject.ShouldBe(subject);
        message.Body.ShouldBeOfType<TextPart>().Text.ShouldBe(htmlBody);
        message.MessageId.ShouldBe(messageId);
    }
}
