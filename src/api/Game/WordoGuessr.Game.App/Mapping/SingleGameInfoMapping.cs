using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class SingleGameInfoMapping
{
    public static ArcadeGameInfoDto MapToArcadeGameInfoDto(this SingleGame game, DateOnly today) =>
        new(
            Id: game.Id,
            Difficulty: game.VersionedGameSource.Difficulty.MapToDto(),
            CreatedAt: game.CreatedAt,
            State: game.StateCode.MapToDto(),
            Word: game.GetDisplayWord(today, out var unavailableReason).MapToDto(unavailableReason));

    public static DailyGameInfoDto MapToDailyGameInfoDto(
        this SingleGame game,
        DateOnly day,
        DateOnly today) =>
        new(
            Day: day,
            State: game.StateCode.MapToDto(),
            GuessedAtGameDay: GuessedAtGameDay(game),
            IsToday: day == today,
            Word: game.GetDisplayWord(today, out var unavailableReason).MapToDto(unavailableReason));

    public static DailyGameInfoDto MapToMissingDailyGameInfoDto(DateOnly day, DateOnly today) =>
        new(
            Day: day,
            State: null,
            GuessedAtGameDay: null,
            IsToday: day == today,
            Word: null);

    private static bool GuessedAtGameDay(SingleGame singleGame) =>
        singleGame.StateCode == SingleGameStateCode.Guessed &&
        singleGame.FinishedAt.HasValue &&
        singleGame.DayOfDailyGame is not null &&
        DailyGameClock.GetDateOnly(singleGame.FinishedAt.Value) == singleGame.DayOfDailyGame.Value;
}
