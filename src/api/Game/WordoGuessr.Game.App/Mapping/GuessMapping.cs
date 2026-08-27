using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GuessMapping
{
    public static GuessDto MapToDto(this Guess guess, double fillPercentage) =>
        new GuessDto(guess.Word.Text, guess.Distance, fillPercentage, guess.Source.MapToDto());

    public static GuessStatusDto MapToDto(this GuessStatus status)
    {
        return status switch
        {
            GuessStatus.NotGuessed =>
                GuessStatusDto.NotGuessed,

            GuessStatus.Guessed =>
                GuessStatusDto.Guessed,

            GuessStatus.AlreadyTried =>
                GuessStatusDto.AlreadyTried,

            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown status")
        };
    }

    public static GuessSourceDto MapToDto(this GuessSource source)
    {
        return source switch
        {
            GuessSource.Player => GuessSourceDto.Player,
            GuessSource.Hint => GuessSourceDto.Hint,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown source")
        };
    }
}
