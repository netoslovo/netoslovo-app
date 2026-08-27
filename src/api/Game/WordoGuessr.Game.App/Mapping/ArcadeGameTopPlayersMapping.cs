using WordoGuessr.Game.App.UseCases.Statistics;
using WordoGuessr.Game.Dto;
using WordoGuessr.Game.ReadModels.Queries;

namespace WordoGuessr.Game.App.Mapping;

internal static class ArcadeGameTopPlayersMapping
{
    public static ArcadeGameTopPlayersDto MapToDto(
        this ArcadeGameTopPlayers streakTop,
        IReadOnlyDictionary<Guid, string> playerNames) =>
            new ArcadeGameTopPlayersDto(
                streakTop.Top.Select(entry => entry.MapToDto(playerNames)).ToArray(),
                streakTop.PlayerTopInfo.MapToDto(playerNames));

    public static ArcadeGameTopPlayersEntryDto MapToDto(
        this ArcadeGameTopPlayersEntry entry,
        IReadOnlyDictionary<Guid, string> playerNames)
    {
        var playerName = playerNames.TryGetValue(entry.PlayerId, out var name)
            ? name
            : Const.UnknownPlayerNamePlaceholder;

        return new ArcadeGameTopPlayersEntryDto(
            Place: entry.Place,
            PlayerName: playerName,
            GuessedGames: entry.GuessedGames,
            AverageScore: entry.AverageScore,
            AverageDuration: entry.AverageDuration);
    }

    public static ArcadeGameTopPlayersCurrentPlayerEntryDto MapToDto(
        this ArcadeGameTopPlayersCurrentPlayerEntry entry,
        IReadOnlyDictionary<Guid, string> playerNames)
    {
        var playerName = playerNames.TryGetValue(entry.PlayerId, out var name)
            ? name
            : Const.UnknownPlayerNamePlaceholder;

        return new ArcadeGameTopPlayersCurrentPlayerEntryDto(
            PlayerId: entry.PlayerId,
            Place: entry.Place,
            PlayerName: playerName,
            GuessedGames: entry.GuessedGames,
            AverageScore: entry.AverageScore,
            AverageDuration: entry.AverageDuration);
    }
}
