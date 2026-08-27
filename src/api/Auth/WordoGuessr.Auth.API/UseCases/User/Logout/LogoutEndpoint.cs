using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.UseCases.User.Logout;

namespace WordoGuessr.Auth.API.UseCases.User.Logout;

internal static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogout(this IEndpointRouteBuilder group)
    {
        group.MapPost("/logout", Handle)
            .WithName("Logout")
            .RequireAuthorization();

        return group;
    }

    private static async Task<Ok> Handle(
        ICommandHandler<LogoutCommand> logoutHandler,
        ICurrentPlayerAccessor currentPlayerAccessor,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(logoutHandler);
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);

        await logoutHandler.Handle(new LogoutCommand(), ct);
        return TypedResults.Ok();
    }
}
