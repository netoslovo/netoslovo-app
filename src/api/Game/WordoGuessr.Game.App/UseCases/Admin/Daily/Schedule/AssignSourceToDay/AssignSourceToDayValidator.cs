using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;

internal sealed class AssignSourceToDayValidator : AbstractValidator<AssignSourceToDayCommand>
{
    public AssignSourceToDayValidator()
    {
        RuleFor(x => x.ApprovedGameSourceId)
            .GreaterThan(0);

        RuleFor(x => x.Day)
            .NotEmpty();
    }
}