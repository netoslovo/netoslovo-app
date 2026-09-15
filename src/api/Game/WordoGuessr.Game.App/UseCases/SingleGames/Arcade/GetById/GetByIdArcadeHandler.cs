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
    private readonly DisplayWordDtoBuilder _displayWordDtoBuilder;

    public GetByIdArcadeHandler(
        IGameStore dbContext,
        IWordsModule wordsModule,
        DisplayWordDtoBuilder displayWordDtoBuilder)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _displayWordDtoBuilder = displayWordDtoBuilder ?? throw new ArgumentNullException(nameof(displayWordDtoBuilder));
    }

    public async Task<GameDto?> Handle(GetByIdArcadeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var singleGame = await _dbContext.ArcadeSingleGamesForDetails()
            .FirstOrDefaultAsync(sg =>
                sg.PlayerId == query.PlayerId &&
                sg.Id == query.GameId,
                ct);

        if (singleGame is null)
        {
            return null;
        }

        var lastGuess = singleGame.StateCode == SingleGameStateCode.Active
            ? singleGame.Guesses.FirstOrDefault()
            : default;

        var totalWordsResult = await _wordsModule.LoadData(new WordsDataRequest(TotalWords: new RequestOperation.TotalWords()), ct);
        var lastGuessDto = lastGuess?.MapToDto(GuessFillPercentageCalculator.Calculate(lastGuess.Distance, totalWordsResult.TotalWords));
        var guessesDto = singleGame.GetGuessesOrderedByDistance()
            .Select(guess => guess.MapToDto(GuessFillPercentageCalculator.Calculate(guess.Distance, totalWordsResult.TotalWords)))
            .ToList();

        return singleGame.MapToDto(
            _displayWordDtoBuilder.Build(singleGame),
            lastGuessDto,
            guessesDto);
    }
}
