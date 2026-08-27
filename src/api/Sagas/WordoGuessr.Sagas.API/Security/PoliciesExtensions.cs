using Microsoft.AspNetCore.Authorization;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;

namespace WordoGuessr.Sagas.API.Security;

internal static class PoliciesExtensions
{
    public static void AddSagasPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Permissions.UploadWordsVersion, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.UploadWordsVersion);
        });

        options.AddPolicy(Permissions.CancelWordsVersionUpload, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.CancelWordsVersionUpload);
        });

        options.AddPolicy(Permissions.ViewWordsVersionUpload, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppClaimTypes.Permission, Permissions.ViewWordsVersionUpload);
        });
    }
}
