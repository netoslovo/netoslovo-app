using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentStreakTop;

internal sealed class GetDailyGameCurrentStreakTopValidator
    : AbstractValidator<GetDailyGameCurrentStreakTopQuery>
{
    private const int MaxTopN = 10;

    public GetDailyGameCurrentStreakTopValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.TopN)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTopN);
    }
}
