using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetDailyGamesSchedule;

internal sealed class GetDailyGamesScheduleValidator : AbstractValidator<GetDailyGamesScheduleQuery>
{
    private const int MaxTake = 50;
    public GetDailyGamesScheduleValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake);

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To);
    }
}