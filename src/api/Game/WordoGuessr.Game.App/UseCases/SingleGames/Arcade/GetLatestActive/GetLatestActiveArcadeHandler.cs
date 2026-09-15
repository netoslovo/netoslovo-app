using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetLatestActive;

internal sealed class GetLatestActiveArcadeHandler : IQueryHandler<GetLatestActiveArcadeQuery, ArcadeGameInfoDto?>
{
    private readonly IGameStore _dbContext;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;

    public GetLatestActiveArcadeHandler(
        IGameStore dbContext,
        DisplayWordDtoBuilder displayWordDtoBuilder)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
    }

    public async Task<ArcadeGameInfoDto?> Handle(GetLatestActiveArcadeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var game = await _dbContext.ArcadeSingleGamesForSummary()
            .Where(sg => sg.PlayerId == query.PlayerId &&
                sg.StateCode == SingleGameStateCode.Active)
            .OrderByDescending(sg => sg.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        if (game is null)
        {
            return null;
        }

        return game.MapToArcadeGameInfoDto(_displayWordDtoBuilder.Build(game));
    }
}
