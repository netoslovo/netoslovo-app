namespace WordoGuessr.Common.Dto;

public sealed record GameSourceDto(long GameSourceId, string Word, string DifficultyCode, int Difficulty);
