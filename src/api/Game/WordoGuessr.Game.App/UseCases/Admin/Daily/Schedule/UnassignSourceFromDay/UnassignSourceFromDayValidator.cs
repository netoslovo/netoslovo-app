using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.UnassignSourceFromDay;

internal sealed class UnassignSourceFromDayValidator : AbstractValidator<UnassignSourceFromDayCommand>
{
    public UnassignSourceFromDayValidator()
    {
        RuleFor(x => x.Day)
            .NotEmpty();
    }
}
