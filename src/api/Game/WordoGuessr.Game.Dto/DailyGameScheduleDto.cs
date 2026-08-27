namespace WordoGuessr.Game.Dto;

public sealed record DailyGameScheduleDto(DateOnly Day, GameSourceDto? GameSource, bool Locked);