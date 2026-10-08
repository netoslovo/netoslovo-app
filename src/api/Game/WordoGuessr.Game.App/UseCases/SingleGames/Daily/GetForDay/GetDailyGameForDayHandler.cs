using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetForDay;

internal sealed class GetDailyGameForDayHandler
    : IQueryHandler<GetDailyGameForDayQuery, Result<GameDto?, GetDailyError>>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;

    public GetDailyGameForDayHandler(
        IGameStore dbContext,
        IWordsModule wordsModule,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Result<GameDto?, GetDailyError>> Handle(GetDailyGameForDayQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        if (query.Day > today)
        {
            return Result<GameDto?, GetDailyError>.Failure(GetDailyError.ScheduleNotFound);
        }

        var dailyGameSource = await _dbContext.SingleGamesDailySchedules
            .Where(s => s.Day == query.Day)
            .Select(s => new { Id = s.ApprovedGameSourceId })
            .FirstOrDefaultAsync(ct);

        if (dailyGameSource is null)
        {
            return Result<GameDto?, GetDailyError>.Failure(GetDailyError.ScheduleNotFound);
        }

        var game = await _dbContext.SingleGamesForDetails()
            .FirstOrDefaultAsync(sg =>
                sg.PlayerId == query.PlayerId &&
                sg.Mode == SingleGameMode.Daily &&
                sg.VersionedGameSource.GameSourceId == dailyGameSource.Id,
                ct);

        if (game is null)
        {
            return Result<GameDto?, GetDailyError>.Success(null);
        }

        var lastGuess = game.LastGuess;
        var requset = new WordsDataRequest(TotalWords: new RequestOperation.TotalWords());
        var totalWordsResult = await _wordsModule.LoadData(requset, ct);
        var lastGuessDto = lastGuess?.MapToDto(totalWordsResult.TotalWords);
        var guessesDto = game.Guesses.MapToListDto(totalWordsResult.TotalWords);

        return Result<GameDto?, GetDailyError>.Success(
            game.MapToDto(today, lastGuessDto, guessesDto));
    }
}
