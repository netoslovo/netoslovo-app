using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using WordoGuessr.Email.Infra.EmailSending;

namespace WordoGuessr.Email.Infra.EmailSending;

internal static class SmtpClientExceptionsHandlingPolicy
{
    public static EmailSenderFailureState GetConnectionFailureState(Exception ex)
    {
        if (ex is SocketException ||
            ex is IOException ||
            ex is SmtpProtocolException)
        {
            return EmailSenderFailureState.Retryable;
        }

        if (ex is SmtpCommandException commandException)
        {
            return GetCommandExceptionFailureState(commandException.StatusCode);
        }

        return EmailSenderFailureState.Permanent;
    }

    public static EmailSenderFailureState GetCommandExceptionFailureState(SmtpStatusCode statusCode)
    {
        var code = (int)statusCode;
        if (code >= 400 && code < 500)
        {
            return EmailSenderFailureState.Retryable;
        }

        if (code >= 500 && code < 600)
        {
            return EmailSenderFailureState.Permanent;
        }

        return EmailSenderFailureState.Unknown;
    }

    public static EmailSenderFailureState GetSendingFailureState(Exception ex)
    {

        if (ex is ServiceNotConnectedException ||
            ex is ServiceNotAuthenticatedException)
        {
            return EmailSenderFailureState.Retryable;
        }

        if (ex is IOException || ex is SmtpProtocolException)
        {
            return EmailSenderFailureState.Unknown;
        }

        if (ex is SmtpCommandException commandException)
        {
            return GetCommandExceptionFailureState(commandException.StatusCode);
        }

        return EmailSenderFailureState.Permanent;
    }

    public static bool ShouldInvalidateClientOnException(Exception ex)
    {
        return
            ex is SmtpProtocolException ||
            ex is ServiceNotConnectedException ||
            ex is ServiceNotAuthenticatedException ||
            ex is SocketException ||
            ex is AuthenticationException ||
            ex is SaslException ||
            ex is IOException;
    }
}