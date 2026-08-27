namespace WordoGuessr.Game.Dto;

public sealed record DailyGamesHistoryDto(IReadOnlyCollection<DailyGameInfoDto> Games, bool HasMore);