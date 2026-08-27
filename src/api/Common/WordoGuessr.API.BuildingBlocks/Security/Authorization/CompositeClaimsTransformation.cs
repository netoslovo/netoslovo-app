using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace WordoGuessr.API.BuildingBlocks.Security.Authorization;

public sealed class CompositeClaimsTransformation : IClaimsTransformation
{
    private readonly IEnumerable<IModuleClaimsTransformation> _moduleClaimsTransformations;

    public CompositeClaimsTransformation(IEnumerable<IModuleClaimsTransformation> moduleClaimsTransformations)
    {
        _moduleClaimsTransformations = moduleClaimsTransformations
            ?? throw new ArgumentNullException(nameof(moduleClaimsTransformations));
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var transformedPrincipal = principal;

        foreach (var moduleClaimsTransformation in _moduleClaimsTransformations)
        {
            transformedPrincipal = await moduleClaimsTransformation.TransformAsync(transformedPrincipal);
        }

        return transformedPrincipal;
    }
}
