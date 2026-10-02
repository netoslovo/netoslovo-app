namespace WordoGuessr.Game.Dto;

public sealed record SharedDailyGameDto(
    GameStateDto GameState,
    IReadOnlyCollection<SharedGuessDto> AllGuesses,
    DisplayWordDto? DisplayWord,
    SharedDailyGameSpoilersDto Spoilers,
    int Score,
    ScoreDetailsDto ScoreDetails,
    string PlayerName,
    SharedGameSpoilersHideReasonDto? SpoilersHideReason,
    DateOnly Day,
    DailyGamePlayerStatsDto? PlayerStats,
    DailyGameStatsDto? GameStats);
