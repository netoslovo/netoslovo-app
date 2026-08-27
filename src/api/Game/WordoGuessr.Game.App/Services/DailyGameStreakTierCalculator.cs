using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Services;

internal static class DailyGameStreakTierCalculator
{
    public static DailyGameStreakTierDto Calculate(int streak)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(streak);

        return streak switch
        {
            0 => DailyGameStreakTierDto.None,
            <= 2 => DailyGameStreakTierDto.Started,
            <= 6 => DailyGameStreakTierDto.Steady,
            <= 13 => DailyGameStreakTierDto.Week,
            <= 29 => DailyGameStreakTierDto.TwoWeeks,
            <= 59 => DailyGameStreakTierDto.Month,
            <= 99 => DailyGameStreakTierDto.Season,
            <= 364 => DailyGameStreakTierDto.Century,
            _ => DailyGameStreakTierDto.Legend
        };
    }
}
