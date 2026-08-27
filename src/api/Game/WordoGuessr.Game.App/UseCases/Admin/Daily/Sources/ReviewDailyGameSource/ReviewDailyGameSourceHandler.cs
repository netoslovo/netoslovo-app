using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.App.Exceptions.Persistence;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;

internal sealed class ReviewDailyGameSourceHandler
    : ICommandHandler<ReviewDailyGameSourceCommand, Result<ReviewDailyGameSourceError>>
{
    private readonly IGameStore _dbContext;
    private readonly IGameStoreUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IWordsModule _wordsModule;

    public ReviewDailyGameSourceHandler(
        IGameStore dbContext,
        IGameStoreUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IWordsModule wordsModule)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
    }

    public async Task<Result<ReviewDailyGameSourceError>> Handle(
        ReviewDailyGameSourceCommand command,
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();

        var wordsVersionResult = await _wordsModule.LoadData(new WordsDataRequest(), ct);
        var wordsVersion = wordsVersionResult.Version;

        var gameSourceExistsInLatestVersion = await _dbContext.VersionedGameSources
            .AnyAsync(gs =>
                gs.GameSourceId == command.GameSourceId &&
                gs.WordsVersion == wordsVersion, ct);

        if (!gameSourceExistsInLatestVersion)
        {
            return Result<ReviewDailyGameSourceError>.Failure(ReviewDailyGameSourceError.GameSourceNotFound);
        }

        var sourceAlreadyAssigned = await _dbContext.SingleGamesDailySchedules
            .AnyAsync(s => s.ApprovedGameSourceId == command.GameSourceId, ct);

        if (!command.Approved && sourceAlreadyAssigned)
        {
            return Result<ReviewDailyGameSourceError>.Failure(ReviewDailyGameSourceError.AlreadyAssignedToGameLocked);
        }

        var review = await _dbContext.DailyGameSourceReviews
            .Include(r => r.ApprovedSource)
            .FirstOrDefaultAsync(r => r.GameSourceId == command.GameSourceId, ct);

        if (review is null)
        {
            review = new DailyGameSourceReview(command.GameSourceId, wordsVersion, now);
            _dbContext.DailyGameSourceReviews.Add(review);
        }

        if (command.Approved)
        {
            var approvedSource = review.Approve(now);
            if (approvedSource is not null)
            {
                _dbContext.ApprovedDailyGameSources.Add(approvedSource);
            }
        }
        else
        {
            var rejectedSource = review.Reject(now);
            if (rejectedSource is not null)
            {
                _dbContext.ApprovedDailyGameSources.Remove(rejectedSource);
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ForeignKeyConstraintViolationException)
        {
            return Result<ReviewDailyGameSourceError>.Failure(ReviewDailyGameSourceError.AlreadyAssignedToGameLocked);
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<ReviewDailyGameSourceError>.Failure(ReviewDailyGameSourceError.ConcurrencyConflict);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<ReviewDailyGameSourceError>.Failure(ReviewDailyGameSourceError.ConcurrencyConflict);
        }

        return Result<ReviewDailyGameSourceError>.Success();
    }
}
