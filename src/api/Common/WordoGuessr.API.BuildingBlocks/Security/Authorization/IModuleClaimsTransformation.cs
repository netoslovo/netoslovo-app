using System.Security.Claims;

namespace WordoGuessr.API.BuildingBlocks.Security.Authorization;

public interface IModuleClaimsTransformation
{
    Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal);
}