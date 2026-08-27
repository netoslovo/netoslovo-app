using WordoGuessr.Game.App.UseCases.SingleGames.Daily;

namespace WordoGuessr.Game.App.Services;

internal static class DailyGameClock
{
    private static readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(DailyGameDefaults.TimeZoneId);

    public static DateOnly GetDateOnly(DateTimeOffset dateTimeOffset)
    {
        var localNow = TimeZoneInfo.ConvertTime(dateTimeOffset, _timeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }
}
