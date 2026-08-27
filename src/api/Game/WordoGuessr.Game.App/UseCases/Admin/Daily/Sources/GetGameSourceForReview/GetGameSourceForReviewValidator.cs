using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetGameSourceForReview;

internal sealed class GetGameSourceForReviewValidator : AbstractValidator<GetGameSourceForReviewQuery>
{
    private const int MaxClosestWordsCount = 100;

    public GetGameSourceForReviewValidator()
    {
        RuleFor(x => x.GameSourceId)
            .GreaterThan(0);

        RuleFor(x => x.ClosestWordsCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(MaxClosestWordsCount);
    }
}
