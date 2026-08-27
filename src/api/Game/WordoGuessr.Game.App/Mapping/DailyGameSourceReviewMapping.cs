using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DailyGameSourceReviewMapping
{
    public static DailyGameSourceReviewDto MapToDto(this DailyGameSourceReview review, bool existsInLatestVersion) =>
        new DailyGameSourceReviewDto(
            review.VersionedGameSource.MapToDto(),
            existsInLatestVersion,
            review.ApprovedSource is not null,
            review.ApprovedSource is not null && review.ApprovedSource.Schedule is not null,
            review.CreatedAt,
            review.UpdatedAt);
}
