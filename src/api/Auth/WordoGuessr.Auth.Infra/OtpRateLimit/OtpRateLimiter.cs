using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Infra.Database;

namespace WordoGuessr.Auth.Infra.OtpRateLimit;

internal sealed class OtpRateLimiter : IOtpRateLimiter
{
    private readonly AuthDbContext _authDbContext;
    private readonly OtpRateLimitOptions _options;

    public OtpRateLimiter(
        AuthDbContext authDbContext,
        IOptions<OtpRateLimitOptions> options)
    {
        _authDbContext = authDbContext ?? throw new ArgumentNullException(nameof(authDbContext));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<bool> TryAcquire(
        EmailAddress address,
        DateTimeOffset at,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(address);

        var cooldownThreshold = at - _options.Cooldown;
        var windowThreshold = at - _options.Window;

        var affectedRows = await _authDbContext.Database
            .ExecuteSqlAsync(
            $"""
            INSERT INTO auth.otp_rate_limits (
                    email,
                    window_started_at,
                    request_count,
                    last_requested_at
                )
                VALUES ({address.Value}, {at}, 1, {at})
                ON CONFLICT (email) DO UPDATE
                SET
                    window_started_at = CASE
                        WHEN otp_rate_limits.window_started_at <= {windowThreshold} THEN {at}
                        ELSE otp_rate_limits.window_started_at
                    END,
                    request_count = CASE
                        WHEN otp_rate_limits.window_started_at <= {windowThreshold} THEN 1
                        ELSE otp_rate_limits.request_count + 1
                    END,
                    last_requested_at = {at}
                WHERE
                    otp_rate_limits.last_requested_at <= {cooldownThreshold}
                    AND (
                    otp_rate_limits.window_started_at <= {windowThreshold}
                    OR otp_rate_limits.request_count < {_options.RequestLimit}
                )
            """,
            ct);

        return affectedRows == 1;
    }
}
