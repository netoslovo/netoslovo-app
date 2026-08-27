namespace WordoGuessr.Game.Dto;

public sealed record DailyGameSourceReviewDto(
    GameSourceDto GameSource,
    bool ExistsInLatestVersion,
    bool Approved,
    bool Scheduled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
