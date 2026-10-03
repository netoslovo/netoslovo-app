namespace WordoGuessr.Game.Dto;

public sealed record DailyGameInfoDto(
    DateOnly Day,
    GameStateDto? State,
    bool? GuessedAtGameDay,
    bool IsToday,
    GameWordDto? GameWord);