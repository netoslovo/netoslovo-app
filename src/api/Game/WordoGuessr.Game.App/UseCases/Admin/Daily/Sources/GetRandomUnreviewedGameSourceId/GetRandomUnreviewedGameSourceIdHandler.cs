using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetRandomUnreviewedGameSourceId;

internal sealed class GetRandomUnreviewedGameSourceIdHandler
    : IQueryHandler<GetRandomUnreviewedGameSourceIdQuery, long?>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;

    public GetRandomUnreviewedGameSourceIdHandler(IGameStore dbContext, IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<long?> Handle(
        GetRandomUnreviewedGameSourceIdQuery query,
        CancellationToken ct)
    {
        var difficulty = Difficulty.FromCode(query.DifficultyCode);

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var gameSourceId = await _dbContext.VersionedGameSources
            .Where(gs =>
                gs.Difficulty == difficulty &&
                !_dbContext.DailyGameSourceReviews.Any(r => r.GameSourceId == gs.GameSourceId) &&
                gs.WordsVersion == wordsVersion)
            .OrderBy(_ => EF.Functions.Random())
            .Select(gs => new { Value = gs.GameSourceId })
            .FirstOrDefaultAsync(ct);

        return gameSourceId?.Value;
    }
}
