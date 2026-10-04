namespace WordoGuessr.Game.Dto;

public sealed record ArcadeGameInfoDto(
    Guid Id,
    DifficultyDto Difficulty,
    DateTimeOffset CreatedAt,
    GameStateDto State,
    GameWordDto GameWord);