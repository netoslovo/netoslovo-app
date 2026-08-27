namespace WordoGuessr.Game.Dto;

public sealed record DailyGameStreakTopCurrentPlayerEntryDto(
    Guid PlayerId,
    int? Place,
    string PlayerName,
    int Streak);
