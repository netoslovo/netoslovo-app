using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using WordoGuessr.API.BuildingBlocks.Diagnostics;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Infra.EmailSending.Exceptions;

namespace WordoGuessr.Email.Infra.EmailSending;

internal sealed class EmailSender : IEmailSender, IDisposable
{
    private readonly EmailSenderOptions _emailOptions;
    private readonly ILogger<EmailSender> _logger;
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly SmtpMetrics _smtpMetrics;
    private readonly SemaphoreSlim _smtpClientLock = new SemaphoreSlim(1, 1);
    private ISmtpClient? _smtpClient;

    public EmailSender(
        IOptions<EmailSenderOptions> emailOptions,
        ISmtpClientFactory smtpClientFactory,
        ILogger<EmailSender> logger,
        SmtpMetrics smtpMetrics)
    {
        _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
        _smtpClientFactory = smtpClientFactory ?? throw new ArgumentNullException(nameof(smtpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smtpMetrics = smtpMetrics ?? throw new ArgumentNullException(nameof(smtpMetrics));
    }

    public async Task SendEmail(EmailAddress to, EmailSubject subject, EmailHtmlBody htmlMessage, Guid messageId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(htmlMessage);

        using var timeoutCts = new CancellationTokenSource(_emailOptions.SendingTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        var linkedToken = linkedCts.Token;
        await AcquireSmtpClientLock(linkedToken, ct, timeoutCts.Token);

        try
        {
            await EnsureSmtpClient(linkedToken, ct, timeoutCts.Token);
            var message = CreateMessage(to, subject, htmlMessage, messageId);
            await SendSmtpMessage(message, linkedToken, ct, timeoutCts.Token);
        }
        finally
        {
            _smtpClientLock.Release();
        }
    }

    public async Task NoOp(CancellationToken ct = default)
    {
        using var timeoutCts = new CancellationTokenSource(_emailOptions.SendingTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        var linkedToken = linkedCts.Token;
        await AcquireSmtpClientLock(linkedToken, ct, timeoutCts.Token);

        try
        {
            if (_smtpClient is null)
            {
                return;
            }

            await ExecuteNoOp(linkedToken, ct, timeoutCts.Token);
        }
        finally
        {
            _smtpClientLock.Release();
        }
    }

    private async Task AcquireSmtpClientLock(
        CancellationToken linkedToken,
        CancellationToken ct,
        CancellationToken timeoutToken)
    {
        try
        {
            using (Duration.Measure(_smtpMetrics.RecordLockWaitDuration))
            {
                await _smtpClientLock.WaitAsync(linkedToken);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            _smtpMetrics.TimeoutOccurred(SmtpMetrics.TimeoutOperation.Lock);
            throw new EmailSenderTimeoutException(ex, EmailSenderFailureState.Retryable);
        }
    }

    private async Task EnsureSmtpClient(
        CancellationToken linkedToken,
        CancellationToken ct,
        CancellationToken timeoutToken)
    {
        try
        {
            if (_smtpClient is null ||
                !_smtpClient.IsConnected ||
                RequiresAuthentication(_emailOptions) && !_smtpClient.IsAuthenticated)
            {
                await InitializeSmtpClient(linkedToken);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            _smtpMetrics.TimeoutOccurred(SmtpMetrics.TimeoutOperation.Connect);
            throw new EmailSenderTimeoutException(ex, EmailSenderFailureState.Retryable);
        }
        catch (Exception ex)
        {
            var failureState = SmtpClientExceptionsHandlingPolicy.GetConnectionFailureState(ex);
            throw new EmailSenderException(ex, failureState);
        }
    }

    private MimeMessage CreateMessage(
        EmailAddress to,
        EmailSubject subject,
        EmailHtmlBody htmlMessage,
        Guid messageId)
    {
        try
        {
            var message = new MimeMessage()
            {
                Body = new TextPart(TextFormat.Html) { Text = htmlMessage.Value },
                MessageId = $"{messageId}@{_emailOptions.FromDomain}",
                Subject = subject.Value
            };

            var fromAddress = $"{_emailOptions.FromBaseAddress}@{_emailOptions.FromDomain}";
            message.From.Add(new MailboxAddress(_emailOptions.FromDisplayName, fromAddress));
            message.To.Add(new MailboxAddress(to.Value, to.Value));
            return message;
        }
        catch (Exception ex)
        {
            throw new EmailSenderException(ex, EmailSenderFailureState.Permanent);
        }
    }

    private async Task SendSmtpMessage(
        MimeMessage message,
        CancellationToken linkedToken,
        CancellationToken ct,
        CancellationToken timeoutToken)
    {
        try
        {
            using (Duration.Measure(_smtpMetrics.RecordSendDuration))
            {
                await _smtpClient!.SendAsync(message, linkedToken);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            InvalidateSmtpClient();
            throw;
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            _smtpMetrics.TimeoutOccurred(SmtpMetrics.TimeoutOperation.Send);
            InvalidateSmtpClient();
            throw new EmailSenderTimeoutException(ex, EmailSenderFailureState.Permanent);
        }
        catch (Exception ex)
        {
            if (SmtpClientExceptionsHandlingPolicy.ShouldInvalidateClientOnException(ex))
            {
                InvalidateSmtpClient();
            }

            var failureState = SmtpClientExceptionsHandlingPolicy.GetSendingFailureState(ex);
            throw new EmailSenderException(ex, failureState);
        }
    }

    private async Task ExecuteNoOp(
        CancellationToken linkedToken,
        CancellationToken ct,
        CancellationToken timeoutToken)
    {
        try
        {
            await _smtpClient!.NoOpAsync(linkedToken);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            InvalidateSmtpClient();
            throw;
        }
        catch (OperationCanceledException ex) when (timeoutToken.IsCancellationRequested)
        {
            _smtpMetrics.TimeoutOccurred(SmtpMetrics.TimeoutOperation.NoOp);
            InvalidateSmtpClient();
            throw new EmailSenderTimeoutException(ex, EmailSenderFailureState.Retryable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending no-op command. Invalidating SmtpClient.");
            InvalidateSmtpClient();
        }
    }

    private async Task InitializeSmtpClient(CancellationToken ct)
    {
        InvalidateSmtpClient();
        _smtpClient = _smtpClientFactory.Create();

        try
        {
            await _smtpClient.ConnectAsync(
                _emailOptions.SmtpServer,
                _emailOptions.SmtpPort,
                _emailOptions.UseSSL,
                ct);

            if (RequiresAuthentication(_emailOptions))
            {
                if (!_smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication))
                {
                    throw new EmailSenderSmtpException("SMTP server does not support authentication");
                }

                await _smtpClient.AuthenticateAsync(
                    _emailOptions.SmtpLogin!,
                    _emailOptions.SmtpPassword!,
                    ct);
            }

            _smtpMetrics.ClientConnected();
            _logger.LogDebug("SmtpClient has been successfully initialized");
        }
        catch
        {
            _smtpMetrics.ConnectionErrorOccurred();
            InvalidateSmtpClient();
            throw;
        }
    }

    private static bool RequiresAuthentication(EmailSenderOptions emailOptions)
    {
        return !string.IsNullOrWhiteSpace(emailOptions.SmtpLogin) &&
            !string.IsNullOrWhiteSpace(emailOptions.SmtpPassword);
    }

    private void InvalidateSmtpClient()
    {
        var smtpClient = Interlocked.Exchange(ref _smtpClient, null);
        if (smtpClient is not null)
        {
            smtpClient.Dispose();
            _smtpMetrics.ClientInvalidated();
        }
    }

    public void Dispose()
    {
        InvalidateSmtpClient();
        _smtpClientLock.Dispose();
    }
}
