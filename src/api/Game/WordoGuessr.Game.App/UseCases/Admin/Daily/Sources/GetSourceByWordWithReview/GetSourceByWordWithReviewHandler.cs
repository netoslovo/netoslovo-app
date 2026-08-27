using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetSourceByWordWithReview;

internal sealed class GetSourceByWordWithReviewHandler
    : IQueryHandler<GetSourceByWordWithReviewQuery, Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;

    public GetSourceByWordWithReviewHandler(IGameStore dbContext, IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>> Handle(GetSourceByWordWithReviewQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var validWord = Word.TryCreate(query.Word, out var word);
        if (!validWord)
        {
            return Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>.Failure(GetSourceByWordWithReviewError.InvalidWord);
        }

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var gameSource = await _dbContext.VersionedGameSources
            .AsNoTracking()
            .Where(gs =>
                gs.GameSource.Word == word &&
                gs.WordsVersion == wordsVersion)
            .Select(gs => new GameSourceDto(
                gs.GameSourceId,
                gs.GameSource.Word.Text,
                new DifficultyDto(gs.Difficulty.Code, gs.Difficulty.Name)))
            .FirstOrDefaultAsync(ct);

        if (gameSource is null)
        {
            return Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>.Failure(GetSourceByWordWithReviewError.GameSourceNotFound);
        }

        var review = await _dbContext.DailyGameSourceReviews
            .AsNoTracking()
            .Where(r => r.GameSourceId == gameSource.GameSourceId)
            .Select(r => new { Approved = r.ApprovedSource != null })
            .FirstOrDefaultAsync(ct);

        return Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>
            .Success(new GameSourceWithReviewDto(
                gameSource,
                review?.Approved
            ));
    }
}
