namespace WordoGuessr.Game.Dto;

public sealed record DailyGameSchedulesDto(
    IReadOnlyCollection<DailyGameScheduleDto> Schedules, bool HasMore);