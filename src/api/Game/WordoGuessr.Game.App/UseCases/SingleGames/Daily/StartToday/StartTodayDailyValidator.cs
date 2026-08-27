using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartToday;

internal sealed class StartTodayDailyValidator : AbstractValidator<StartTodayDailyCommand>
{
    public StartTodayDailyValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
