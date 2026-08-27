using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;

internal sealed class AutoAssignSourcesToEmptyDaysValidator
    : AbstractValidator<AutoAssignSourcesToEmptyDaysCommand>
{
    public AutoAssignSourcesToEmptyDaysValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To);
    }
}