using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.CurrentPlayerAccessor;

internal sealed class GuestSessionManager : IGuestSessionManager
{
    private const string GuestCookieName = ".WordoGuessr.GuestSession";
    private const string ProtectionPurpose = "GuestPlayerSessionCookie.v1";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger _logger;

    private Guid? _cachedGuestSessionId;

    public GuestSessionManager(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GuestSessionManager> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _dataProtector = dataProtectionProvider.CreateProtector(ProtectionPurpose);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Guid GetOrCreateGuestPlayerSession()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new CurrentPlayerException("Current player is unavailable from outside of HTTP request.");

        if (_cachedGuestSessionId.HasValue)
        {
            return _cachedGuestSessionId.Value;
        }

        var guestIdFromCookieRaw = httpContext.Request.Cookies[GuestCookieName];
        if (!string.IsNullOrWhiteSpace(guestIdFromCookieRaw))
        {
            try
            {
                var unprotectedValue = _dataProtector.Unprotect(guestIdFromCookieRaw);
                var cookieGuestSessionId = Guid.Parse(unprotectedValue);
                _cachedGuestSessionId = cookieGuestSessionId;
                return cookieGuestSessionId;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    ex,
                    "Guest session cookie is invalid; a new guest session will be created");
            }
        }

        var guestSessionId = Guid.CreateVersion7();
        var protectedGuestSessionId = _dataProtector.Protect(guestSessionId.ToString());

        httpContext.Response.Cookies.Append(
            GuestCookieName,
            protectedGuestSessionId,
            CreateCookieOptions());

        _cachedGuestSessionId = guestSessionId;

        return guestSessionId;
    }

    public void ClearGuestSession()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new CurrentPlayerException("Current player is unavailable from outside of HTTP request.");

        _cachedGuestSessionId = null;
        httpContext.Response.Cookies.Delete(
            GuestCookieName,
            CreateCookieOptions());
    }

    private static CookieOptions CreateCookieOptions() => new CookieOptions
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Strict,
        Secure = true
    };
}
