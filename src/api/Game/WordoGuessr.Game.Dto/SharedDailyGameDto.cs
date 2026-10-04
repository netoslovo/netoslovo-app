namespace WordoGuessr.Game.Dto;

public sealed record SharedDailyGameDto(
    GameStateDto GameState,
    IReadOnlyCollection<SharedGuessDto> AllGuesses,
    SharedDailyGameSpoilersDto Spoilers,
    int Score,
    ScoreDetailsDto ScoreDetails,
    string PlayerName,
    DateOnly Day,
    DailyGamePlayerStatsDto? PlayerStats,
    DailyGameStatsDto? GameStats);
