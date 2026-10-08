using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GuessMapping
{
    public static GuessDto MapToDto(this Guess guess, ushort totalWords) =>
        MapToDto(guess.Word.Text, guess.Distance, guess.Source, totalWords);

    public static GuessDto MapToDto(this GuessAttempt attempt, ushort totalWords) =>
        MapToDto(attempt.Word.Text, attempt.Distance, attempt.Source, totalWords);

    public static IReadOnlyList<GuessListEntryDto> MapToListDto(
        this IEnumerable<Guess> guesses,
        ushort totalWords) =>
            guesses
                .OrderBy(guess => guess.Id)
                .Select((guess, index) => new GuessListEntryDto(
                    guess.MapToDto(totalWords),
                    index + 1))
                .OrderBy(entry => entry.Guess.Distance)
                .ToArray();

    public static IReadOnlyList<SharedGuessDto> MapToSharedListDto(
        this IEnumerable<Guess> guesses,
        ushort totalWords,
        bool showWords) =>
            guesses
                .OrderBy(guess => guess.Id)
                .Select((guess, index) =>
                {
                    var guessDto = guess.MapToDto(totalWords);
                    return new SharedGuessDto(
                        showWords ? guessDto.Word : null,
                        guessDto.Distance,
                        index + 1,
                        guessDto.FillPercentage,
                        guessDto.Source);
                })
                .ToArray();

    public static IReadOnlyList<VisibleSharedGuessDto> MapToVisibleSharedListDto(
        this IEnumerable<Guess> guesses,
        ushort totalWords) =>
            guesses
                .OrderBy(guess => guess.Id)
                .Select((guess, index) =>
                {
                    var guessDto = guess.MapToDto(totalWords);
                    return new VisibleSharedGuessDto(
                        guessDto.Word,
                        guessDto.Distance,
                        index + 1,
                        guessDto.FillPercentage,
                        guessDto.Source);
                })
                .ToArray();

    public static IReadOnlyList<HiddenSharedGuessDto> MapToHiddenSharedListDto(
        this IEnumerable<Guess> guesses,
        ushort totalWords) =>
            guesses
                .OrderBy(guess => guess.Id)
                .Select((guess, index) =>
                {
                    var guessDto = guess.MapToDto(totalWords);
                    return new HiddenSharedGuessDto(
                        guessDto.Distance,
                        index + 1,
                        guessDto.FillPercentage,
                        guessDto.Source);
                })
                .ToArray();

    private static GuessDto MapToDto(
        string word,
        int distance,
        GuessSource source,
        ushort totalWords) =>
            new GuessDto(
                word,
                distance,
                GuessFillPercentageCalculator.Calculate(distance, totalWords),
                source.MapToDto());

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
