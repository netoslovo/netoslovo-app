using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;

internal sealed class GetDailyGameCurrentPlayerStreakValidator
    : AbstractValidator<GetDailyGameCurrentPlayerStreakQuery>
{
    public GetDailyGameCurrentPlayerStreakValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}