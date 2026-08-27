namespace WordoGuessr.Game.Dto;

public sealed record GuessOutcomeDto(
    GuessStatusDto GuessStatus,
    GuessDto CurrentGuess,
    IReadOnlyCollection<GuessDto> AllGuesses,
    int Score,
    ScoreDetailsDto ScoreDetails);
