using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetShare;

internal sealed class GetShareHandler : IQueryHandler<GetShareQuery, DailyGameShareDto?>
{
    private readonly IGameStore _dbContext;

    public GetShareHandler(IGameStore dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<DailyGameShareDto?> Handle(GetShareQuery query, CancellationToken ct)
    {
        var share = await _dbContext.DailyGameShares
            .AsNoTracking()
            .SingleOrDefaultAsync(
                sgs =>
                    sgs.Id == query.GameId &&
                    sgs.SingleGame.PlayerId == query.PlayerId &&
                    sgs.SingleGame.Mode == Domain.SingleGameMode.Daily,
                ct);

        return share?.MapToDto();
    }
}
