using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Dto;
using WordoGuessr.Auth.App.UseCases.User.GetProfile;

namespace WordoGuessr.Auth.API.UseCases.User.GetProfile;

internal static class GetProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetProfile(this IEndpointRouteBuilder group)
    {
        group.MapGet("/profile", Handle)
            .WithName("GetProfile")
            .RequireAuthorization();

        return group;
    }

    private static async Task<Results<Ok<ProfileDto>, NotFound>> Handle(
        IQueryHandler<GetProfileQuery, ProfileDto?> handler,
        ICurrentPlayerAccessor currentPlayerAccessor,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var profile = await handler.Handle(new GetProfileQuery(currentPlayer.PlayerId), ct);

        return profile is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(profile);
    }
}
