namespace WordoGuessr.Game.Dto;

public sealed record DailyGameSourceReviewsDto(IReadOnlyCollection<DailyGameSourceReviewDto> Reviews, bool HasMore);
