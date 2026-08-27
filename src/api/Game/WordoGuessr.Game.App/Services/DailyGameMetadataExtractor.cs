using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.Services;

internal sealed class DailyGameMetadataExtractor
{
    private readonly TimeProvider _timeProvider;

    public DailyGameMetadataExtractor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DailyGameMetadata? ExtractOrDefault(SingleGame game)
    {
        if (game.Mode != SingleGameMode.Daily) return null;

        if (game.DayOfDailyGame is null) return null;

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);
        return new DailyGameMetadata(game.DayOfDailyGame.Value, today);
    }
}