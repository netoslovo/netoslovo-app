using WordoGuessr.Game.App.UseCases.Statistics;
using WordoGuessr.Game.Dto;
using WordoGuessr.Game.ReadModels.Queries;

namespace WordoGuessr.Game.App.Mapping;

internal static class DailyGameStreakTopMapping
{
    public static DailyGameStreakTopDto MapToDto(
        this DailyGameStreakTop streakTop,
        IReadOnlyDictionary<Guid, string> playerNames) =>
            new DailyGameStreakTopDto(
                streakTop.Top.Select(entry => entry.MapToDto(playerNames)).ToArray(),
                streakTop.PlayerStreakTopInfo.MapToDto(playerNames));

    public static DailyGameStreakTopEntryDto MapToDto(
        this DailyGameStreakTopEntry entry,
        IReadOnlyDictionary<Guid, string> playerNames)
    {
        var playerName = playerNames.TryGetValue(entry.PlayerId, out var name)
            ? name
            : Const.UnknownPlayerNamePlaceholder;

        return new DailyGameStreakTopEntryDto(entry.Place, playerName, entry.Streak);
    }

    public static DailyGameStreakTopCurrentPlayerEntryDto MapToDto(
        this DailyGameStreakTopCurrentPlayerEntry entry,
        IReadOnlyDictionary<Guid, string> playerNames)
    {
        var playerName = playerNames.TryGetValue(entry.PlayerId, out var name)
            ? name
            : Const.UnknownPlayerNamePlaceholder;

        return new DailyGameStreakTopCurrentPlayerEntryDto(
            entry.PlayerId,
            entry.Place,
            playerName,
            entry.Streak);
    }
}
