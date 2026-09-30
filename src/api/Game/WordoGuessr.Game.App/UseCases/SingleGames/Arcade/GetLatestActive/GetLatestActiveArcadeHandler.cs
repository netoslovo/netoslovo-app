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
    private readonly TimeProvider _timeProvider;

    public GetLatestActiveArcadeHandler(
        IGameStore dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ArcadeGameInfoDto?> Handle(GetLatestActiveArcadeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);
        var game = await _dbContext.SingleGamesForSummary()
            .Where(sg => sg.PlayerId == query.PlayerId &&
                sg.StateCode == SingleGameStateCode.Active &&
                sg.Mode == SingleGameMode.Arcade)
            .OrderByDescending(sg => sg.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        if (game is null)
        {
            return null;
        }

        return game.MapToArcadeGameInfoDto(today);
    }
}
