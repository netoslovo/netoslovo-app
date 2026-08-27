using Microsoft.AspNetCore.Authorization;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;

namespace WordoGuessr.Game.API.Security;

internal static class PoliciesExtensions
{
    public static void AddGamePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Permissions.ViewSources, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.ViewSources);
        });

        options.AddPolicy(Permissions.AssignSource, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.AssignSource);
        });

        options.AddPolicy(Permissions.UnassignSource, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.UnassignSource);
        });

        options.AddPolicy(Permissions.SetSourceReview, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.SetSourceReview);
        });
    }
}
