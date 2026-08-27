namespace WordoGuessr.Game.Dto;

public sealed record GameSourceWithReviewDto(GameSourceDto GameSource, bool? Approved);
