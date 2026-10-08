using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetById;

internal sealed class GetByIdArcadeHandler : IQueryHandler<GetByIdArcadeQuery, GameDto?>
{
    private readonly IGameStore _dbContext;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;

    public GetByIdArcadeHandler(
        IGameStore dbContext,
        IWordsModule wordsModule,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<GameDto?> Handle(GetByIdArcadeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);
        var singleGame = await _dbContext.SingleGamesForDetails()
            .FirstOrDefaultAsync(sg =>
                sg.PlayerId == query.PlayerId &&
                sg.Id == query.GameId &&
                sg.Mode == SingleGameMode.Arcade,
                ct);

        if (singleGame is null)
        {
            return null;
        }

        var lastGuess = singleGame.StateCode == SingleGameStateCode.Active
            ? singleGame.LastGuess
            : default;

        var totalWordsResult = await _wordsModule.LoadData(new WordsDataRequest(TotalWords: new RequestOperation.TotalWords()), ct);
        var lastGuessDto = lastGuess?.MapToDto(totalWordsResult.TotalWords);
        var guessesDto = singleGame.Guesses.MapToListDto(totalWordsResult.TotalWords);

        return singleGame.MapToDto(today, lastGuessDto, guessesDto);
    }
}
