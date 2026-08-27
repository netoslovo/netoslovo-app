namespace WordoGuessr.Game.ReadModels.Queries;

public sealed record ArcadeGameTopPlayers(
    IReadOnlyList<ArcadeGameTopPlayersEntry> Top,
    ArcadeGameTopPlayersCurrentPlayerEntry PlayerTopInfo);
