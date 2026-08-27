using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Game.App.UseCases.Statistics.GetArcadeGameTopPlayers;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.API.UseCases.Statistics.GetArcadeGameTopPlayers;

public static class GetArcadeGameTopPlayersEndpoint
{
    public static IEndpointRouteBuilder MapGetArcadeGameTopPlayers(this IEndpointRouteBuilder group)
    {
        // TODO: route and naming
        group.MapGet("/arcade/stats/players/top", Handle)
            .WithName("GetArcadeGameTopPlayers");

        return group;
    }

    private static async Task<Ok<ArcadeGameTopPlayersDto>> Handle(
        [FromQuery] string difficultyCode,
        [FromQuery] int topN,
        [FromServices] ICurrentPlayerAccessor currentPlayerAccessor,
        [FromServices] IQueryHandler<GetArcadeGameTopPlayersQuery, ArcadeGameTopPlayersDto> handler,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentPlayerAccessor);
        ArgumentNullException.ThrowIfNull(handler);

        var currentPlayer = currentPlayerAccessor.GetCurrentPlayer();
        var query = new GetArcadeGameTopPlayersQuery(currentPlayer.PlayerId, difficultyCode, topN);
        var top = await handler.Handle(query, ct);

        return TypedResults.Ok(top);
    }
}
