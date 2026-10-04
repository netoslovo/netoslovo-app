namespace WordoGuessr.Game.Dto;

public sealed record GameDto(
    Guid Id,
    DifficultyDto Difficulty,
    GameStateDto GameState,
    GuessDto? CurrentGuess,
    IReadOnlyCollection<GuessDto> AllGuesses,
    GameWordDto GameWord,
    HintsInfoDto HintInfo,
    int Score,
    ScoreDetailsDto ScoreDetails);
