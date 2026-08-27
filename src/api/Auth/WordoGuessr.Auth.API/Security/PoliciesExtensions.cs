using Microsoft.AspNetCore.Authorization;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;

namespace WordoGuessr.Auth.API.Security;

internal static class PoliciesExtensions
{
    public static void AddAuthPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Permissions.ManageUserNameFilter, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.ManageUserNameFilter);
        });
    }
}
