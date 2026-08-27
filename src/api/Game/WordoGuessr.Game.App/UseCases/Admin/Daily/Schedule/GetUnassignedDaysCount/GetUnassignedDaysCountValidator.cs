using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetUnassignedDaysCount;

internal sealed class GetUnassignedDaysCountValidator
    : AbstractValidator<GetUnassignedDaysCountQuery>
{
    public GetUnassignedDaysCountValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To);
    }
}
