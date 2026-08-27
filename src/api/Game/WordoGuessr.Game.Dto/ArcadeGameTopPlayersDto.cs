namespace WordoGuessr.Game.Dto;

public sealed record ArcadeGameTopPlayersDto(
    IReadOnlyList<ArcadeGameTopPlayersEntryDto> top,
    ArcadeGameTopPlayersCurrentPlayerEntryDto PlayerTopInfo);
