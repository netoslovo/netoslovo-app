namespace WordoGuessr.Game.Dto;

public sealed record GameDto(
    Guid Id,
    DifficultyDto Difficulty,
    GameStateDto GameState,
    GuessDto? CurrentGuess,
    IReadOnlyCollection<GuessDto> AllGuesses,
    DisplayWordDto DisplayWord,
    SecretWordDto SecretWord,
    HintsInfoDto HintInfo,
    int Score,
    ScoreDetailsDto ScoreDetails);
