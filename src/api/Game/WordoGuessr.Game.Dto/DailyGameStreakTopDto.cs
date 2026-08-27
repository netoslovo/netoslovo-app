namespace WordoGuessr.Game.Dto;

public sealed record DailyGameStreakTopDto(
    IReadOnlyList<DailyGameStreakTopEntryDto> Top,
    DailyGameStreakTopCurrentPlayerEntryDto CurrentPlayerTopInfo
);
