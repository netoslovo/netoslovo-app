using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;

internal sealed class ReviewDailyGameSourceValidator : AbstractValidator<ReviewDailyGameSourceCommand>
{
    public ReviewDailyGameSourceValidator()
    {
        RuleFor(x => x.GameSourceId)
            .GreaterThan(0);
    }
}
