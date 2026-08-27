namespace WordoGuessr.Game.Dto;

public sealed record DailyGameStreakTopEntryDto(
    int Place,
    string PlayerName,
    int Streak
);
