using System.Security.Claims;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;

namespace WordoGuessr.Sagas.API.Security;

internal sealed class SagasPermissionClaimsTransformation : IModuleClaimsTransformation
{
    private static readonly string[] _adminPermissions =
    [
        Permissions.UploadWordsVersion,
        Permissions.CancelWordsVersionUpload,
        Permissions.ViewWordsVersionUpload
    ];

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.IsInRole(AppRoles.Admin))
        {
            return Task.FromResult(principal);
        }

        var assignedPermissions = principal.FindAll(AppClaimTypes.Permission)
            .Select(claim => claim.Value)
            .ToHashSet();

        var permissionClaims = _adminPermissions
            .Where(permission => !assignedPermissions.Contains(permission))
            .Select(permission => new Claim(AppClaimTypes.Permission, permission))
            .ToArray();

        if (permissionClaims.Length > 0)
        {
            principal.AddIdentity(new ClaimsIdentity(permissionClaims));
        }

        return Task.FromResult(principal);
    }
}
