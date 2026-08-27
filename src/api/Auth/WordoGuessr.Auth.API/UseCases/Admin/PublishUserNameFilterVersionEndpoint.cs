using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.API.Security;
using WordoGuessr.Auth.App.UseCases.Admin.PublishNewUserNameFilterVersion;

namespace WordoGuessr.Auth.API.UseCases.Admin;

internal static class PublishUserNameFilterVersionEndpoint
{
    public static IEndpointRouteBuilder MapPublishUserNameFilterVersion(this IEndpointRouteBuilder group)
    {
        group.MapPost("/username-filter/version/{version:int}", Handle)
            .WithName("PublishUserNameFilterVersion")
            .RequireAuthorization(Permissions.ManageUserNameFilter);

        return group;
    }

    private static async Task<Ok> Handle(
        [FromRoute] int version,
        [FromServices] ICommandHandler<PublishNewUserNameFilterVersionCommand> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        await handler.Handle(new PublishNewUserNameFilterVersionCommand(version), ct);
        return TypedResults.Ok();
    }
}
