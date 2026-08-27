using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetDailySourceReviews;

internal sealed class GetDailySourceReviewsValidator : AbstractValidator<GetDailySourceReviewsQuery>
{
    private const int MaxTake = 50;
    public GetDailySourceReviewsValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake);

        RuleFor(x => x.WordFilter)
            .NotEmpty()
            .When(x => x.WordFilter is not null);
    }
}