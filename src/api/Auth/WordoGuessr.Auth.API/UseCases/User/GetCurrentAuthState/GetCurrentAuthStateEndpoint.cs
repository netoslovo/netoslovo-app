using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Dto;
using WordoGuessr.Auth.App.UseCases.User.GetCurrentAuthState;

namespace WordoGuessr.Auth.API.UseCases.User.GetCurrentAuthState;

internal static class GetCurrentAuthStateEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentAuthState(this IEndpointRouteBuilder group)
    {
        group.MapGet("/me", Handle)
            .WithName("GetCurrentAuthState");

        return group;
    }

    private static async Task<Ok<AuthMeDto>> Handle(
        IQueryHandler<GetCurrentAuthStateQuery, AuthMeDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var authState = await handler.Handle(new GetCurrentAuthStateQuery(), ct);
        return TypedResults.Ok(authState);
    }
}
