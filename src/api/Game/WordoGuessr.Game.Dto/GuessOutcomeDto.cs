namespace WordoGuessr.Game.Dto;

public sealed record GuessOutcomeDto(
    GuessStatusDto GuessStatus,
    GuessDto CurrentGuess,
    IReadOnlyList<GuessListEntryDto> AllGuesses,
    int Score,
    ScoreDetailsDto ScoreDetails);
