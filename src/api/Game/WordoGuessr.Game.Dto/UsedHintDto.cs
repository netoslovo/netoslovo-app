namespace WordoGuessr.Game.Dto;

public sealed record UsedHintDto(
    HintTypeDto Type,
    int Penalty,
    DateTimeOffset UsedAt);