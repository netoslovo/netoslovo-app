namespace WordoGuessr.Game.Dto;

public sealed record SharedDailyGameDto(
    GameStateDto GameState,
    SharedGuessDto? CurrentGuess,
    IReadOnlyCollection<SharedGuessDto> AllGuesses,
    DisplayWordDto? DisplayWord,
    int Score,
    ScoreDetailsDto ScoreDetails,
    string PlayerName,
    SharedGameSpoilersHideReason? SpoilersHideReason,
    DateOnly Day,
    DailyGamePlayerStatsDto? PlayerStats,
    DailyGameStatsDto? GameStats);
