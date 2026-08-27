using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;

internal sealed class GetArcadeGamesHistoryHandler
    : IQueryHandler<GetArcadeGamesHistoryQuery, ArcadeGamesHistoryDto>
{
    private readonly IGameStore _dbContext;
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;

    public GetArcadeGamesHistoryHandler(
        IGameStore dbContext,
        DisplayWordDtoBuilder displayWordDtoBuilder)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
    }

    public async Task<ArcadeGamesHistoryDto> Handle(GetArcadeGamesHistoryQuery query, CancellationToken ct)
    {
        var gamesFromDb = await _dbContext.SingleGamesForSummary()
            .Where(sg =>
                sg.PlayerId == query.PlayerId &&
                sg.Mode == Domain.SingleGameMode.Arcade)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .ToArrayAsync(ct);

        var hasMore = gamesFromDb.Length > query.Take;
        var games = gamesFromDb
            .Take(query.Take)
            .Select(game => game.MapToArcadeGameInfoDto(_displayWordDtoBuilder.Build(game)))
            .ToArray();

        return new ArcadeGamesHistoryDto(games, hasMore);
    }
}
