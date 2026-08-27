using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.UseCases.Difficulties;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Dictionaries.Difficulties;

public static class GetDifficultiesEndpoint
{
    public static IEndpointRouteBuilder MapGetDifficulties(this IEndpointRouteBuilder group)
    {
        group.MapGet("/difficulties", Handle)
            .WithName("GetDifficulties");

        return group;
    }

    private static async Task<Ok<IReadOnlyCollection<DifficultyDto>>> Handle(
        [FromServices] IQueryHandler<GetDifficultiesQuery, IReadOnlyCollection<DifficultyDto>> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await handler.Handle(new GetDifficultiesQuery(), ct);
        return TypedResults.Ok(result);
    }
}
