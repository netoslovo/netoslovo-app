using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class IdentitySessionManager : ISessionManager
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentitySessionManager(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    public async Task RefreshSignIn(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"Auth user '{userId}' was not found.");

        await _signInManager.RefreshSignInAsync(appUser);
    }

    public async Task SignIn(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"Auth user '{userId}' was not found.");
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(appUser.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, appUser.Email));
        }

        await _signInManager.SignInWithClaimsAsync(appUser, isPersistent: true, claims);
    }

    public Task SignOut() => _signInManager.SignOutAsync();
}
