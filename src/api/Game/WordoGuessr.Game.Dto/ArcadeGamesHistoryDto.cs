namespace WordoGuessr.Game.Dto;

public sealed record ArcadeGamesHistoryDto(IReadOnlyCollection<ArcadeGameInfoDto> Games, bool HasMore);