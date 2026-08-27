using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.Services;

internal sealed class DisplayWordVisibilityPolicy
{
    public SingleGameStateCode State { get; }
    public DailyGameMetadata? DailyGameMetadata { get; }

    public DisplayWordVisibilityPolicy(
        SingleGameStateCode state,
        DailyGameMetadata? dailyGameMetadata)
    {
        State = state;
        DailyGameMetadata = dailyGameMetadata;
    }

    public bool ShouldShowRevealedWord(out DisplayWordHideReason? hideReason)
    {
        var isCurrentDailyGame =
            DailyGameMetadata is not null &&
            DailyGameMetadata.GameDay == DailyGameMetadata.Today;

        if (State == SingleGameStateCode.Guessed)
        {
            hideReason = null;
            return true;
        }

        if (State == SingleGameStateCode.Surrendered && isCurrentDailyGame)
        {
            hideReason = DisplayWordHideReason.HiddenForToday;
            return false;
        }

        if (State == SingleGameStateCode.Surrendered)
        {
            hideReason = null;
            return true;
        }

        hideReason = null;
        return false;
    }
}

internal enum DisplayWordHideReason
{
    HiddenForToday
}
