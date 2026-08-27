using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartForDay;

internal sealed class StartForDayDailyValidator : AbstractValidator<StartForDayDailyCommand>
{
    public StartForDayDailyValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
