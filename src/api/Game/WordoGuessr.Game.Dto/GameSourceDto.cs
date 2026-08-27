namespace WordoGuessr.Game.Dto;

public sealed record GameSourceDto(long GameSourceId, string Word, DifficultyDto Difficulty);
