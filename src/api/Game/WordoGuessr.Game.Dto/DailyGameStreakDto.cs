namespace WordoGuessr.Game.Dto;

public sealed record DailyGameStreakDto(
    int Streak,
    DailyGameStreakTierDto Tier
);