using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetRandomUnreviewedGameSourceId;

namespace WordoGuessr.Game.API.UseCases.Admin.Daily.GetRandomUnreviewedGameSourceId;

internal static class GetRandomUnreviewedGameSourceIdEndpoint
{
    public static IEndpointRouteBuilder MapGetRandomUnreviewedGameSourceId(this IEndpointRouteBuilder group)
    {
        group.MapGet("/sources/unreviewed/random-id", Handle)
            .RequireAuthorization(Permissions.ViewSources)
            .ProducesValidationProblem()
            .WithName("GetRandomUnreviewedGameSourceId");

        return group;
    }

    private static async Task<Ok<GetRandomUnreviewedGameSourceIdResponse>> Handle(
        [FromQuery] string difficultyCode,
        [FromServices] IQueryHandler<GetRandomUnreviewedGameSourceIdQuery, long?> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var query = new GetRandomUnreviewedGameSourceIdQuery(difficultyCode);
        var gameSourceId = await handler.Handle(query, ct);

        return TypedResults.Ok(new GetRandomUnreviewedGameSourceIdResponse(gameSourceId));
    }
}
