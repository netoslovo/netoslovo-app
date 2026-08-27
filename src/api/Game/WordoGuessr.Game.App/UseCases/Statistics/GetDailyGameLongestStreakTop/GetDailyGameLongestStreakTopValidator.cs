using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameLongestStreakTop;

internal sealed class GetDailyGameLongestStreakTopValidator
    : AbstractValidator<GetDailyGameLongestStreakTopQuery>
{
    private const int MaxTopN = 10;

    public GetDailyGameLongestStreakTopValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.TopN)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTopN);
    }
}
