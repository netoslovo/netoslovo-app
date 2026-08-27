using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGamePlayerStats;

internal sealed class GetDailyGamePlayerStatsValidator : AbstractValidator<GetDailyGamePlayerStatsQuery>
{
    public GetDailyGamePlayerStatsValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
