using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetApprovedUnassignedDailySourcesCount;

internal sealed class GetApprovedUnassignedDailySourcesCountHandler
    : IQueryHandler<GetApprovedUnassignedDailySourcesCountQuery, int>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;

    public GetApprovedUnassignedDailySourcesCountHandler(IGameStore dbContext, IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<int> Handle(GetApprovedUnassignedDailySourcesCountQuery query, CancellationToken ct)
    {
        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var count = await _dbContext.ApprovedDailyGameSources
            .CountAsync(
                source =>
                    !_dbContext.SingleGamesDailySchedules
                        .Any(schedule => schedule.ApprovedGameSourceId == source.GameSourceId) &&
                    _dbContext.VersionedGameSources.Any(vgs =>
                        vgs.GameSourceId == source.GameSourceId &&
                        vgs.WordsVersion == wordsVersion),
                ct);

        return count;
    }
}
