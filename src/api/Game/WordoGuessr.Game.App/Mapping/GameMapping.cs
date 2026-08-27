using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GameMapping
{
    public static GameDto MapToDto(
        this SingleGame singleGame,
        DisplayWordDto displayWord,
        GuessDto? lastGuess = default,
        IReadOnlyCollection<GuessDto>? guesses = default)
    {
        var score = singleGame.GetScore();

        return new GameDto(
            singleGame.Id,
            singleGame.VersionedGameSource.Difficulty.MapToDto(),
            singleGame.StateCode.MapToDto(),
            lastGuess,
            guesses ?? [],
            displayWord,
            Helpers.BuildHintsInfo(singleGame),
            score.Value,
            score.MapToDto());
    }

    public static GameStateDto MapToDto(this SingleGameStateCode state)
    {
        return state switch
        {
            SingleGameStateCode.Active => GameStateDto.Active,
            SingleGameStateCode.Guessed => GameStateDto.Guessed,
            SingleGameStateCode.Surrendered => GameStateDto.Surrendered,
            SingleGameStateCode.Cancelled => GameStateDto.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown state")
        };
    }
}
