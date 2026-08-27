using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetGameSourceForReview;

internal sealed class GetGameSourceForReviewHandler
    : IQueryHandler<GetGameSourceForReviewQuery, Result<UnreviewedSourceDto, GetGameSourceForReviewError>>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;

    public GetGameSourceForReviewHandler(IGameStore dbContext, IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<UnreviewedSourceDto, GetGameSourceForReviewError>> Handle(
        GetGameSourceForReviewQuery query,
        CancellationToken ct)
    {
        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var gameSource = await _dbContext.VersionedGameSources
            .Where(gs =>
                gs.GameSourceId == query.GameSourceId &&
                gs.WordsVersion == wordsVersion)
            .Select(gs => new
            {
                gs.GameSourceId,
                gs.GameSource.Word,
                Difficulty = gs.Difficulty.MapToDto()
            })
            .FirstOrDefaultAsync(ct);

        if (gameSource is null)
        {
            return Result<UnreviewedSourceDto, GetGameSourceForReviewError>.Failure(
                GetGameSourceForReviewError.GameSourceNotFound);
        }

        var request = new WordsDataRequest(
            ClosestWords: new RequestOperation.ClosestWords(
                gameSource.Word,
                checked((ushort)query.ClosestWordsCount)));

        var closestWordsResult = await _wordsModule.LoadData(request, ct);

        if (!closestWordsResult.ClosestWords.IsSuccess)
        {
            return Result<UnreviewedSourceDto, GetGameSourceForReviewError>.Failure(
                GetGameSourceForReviewError.ClosestWordsCouldNotBeLoaded);
        }

        var closestWords = closestWordsResult.ClosestWords.Value.Select(w => w.Text).ToArray();
        var gameSourceDto = new GameSourceDto(gameSource.GameSourceId, gameSource.Word.Text, gameSource.Difficulty);

        return Result<UnreviewedSourceDto, GetGameSourceForReviewError>.Success(
            new UnreviewedSourceDto(gameSourceDto, closestWords));
    }
}
