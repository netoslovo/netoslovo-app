using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.CurrentPlayerAccessor;

internal sealed class HttpContextCurrentPlayerAccessor : ICurrentPlayerAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGuestSessionManager _guestSessionManager;

    public HttpContextCurrentPlayerAccessor(IHttpContextAccessor httpContextAccessor, IGuestSessionManager guestSessionManager)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _guestSessionManager = guestSessionManager ?? throw new ArgumentNullException(nameof(guestSessionManager));
    }

    public CurrentPlayer GetCurrentPlayer()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var rawUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(rawUserId, out var userId))
                {
                    var email = user.FindFirstValue(ClaimTypes.Email);
                    var userName = user.FindFirstValue(ClaimTypes.Name)
                        ?? user.Identity?.Name;

                    var roles = user.FindAll(ClaimTypes.Role)
                        .Select(x => x.Value)
                        .Distinct()
                        .ToArray();

                    var permissions = user.FindAll(AppClaimTypes.Permission)
                        .Select(x => x.Value)
                        .Distinct()
                        .ToArray();

                    return CurrentPlayer.AuthenticatedUser(userId, userName, email, roles, permissions);
                }
                else
                {
                    throw new CurrentPlayerException("Invalid NameIdentifier claim provided");
                }
            }

            var guestSessionId = _guestSessionManager.GetOrCreateGuestPlayerSession();
            return CurrentPlayer.Guest(guestSessionId);
        }
        catch (Exception ex)
        {
            throw new CurrentPlayerException(ex);
        }
    }
}
