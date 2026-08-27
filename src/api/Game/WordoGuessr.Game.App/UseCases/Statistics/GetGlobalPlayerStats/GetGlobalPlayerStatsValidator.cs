using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetGlobalPlayerStats;

internal sealed class GetGlobalPlayerStatsValidator : AbstractValidator<GetGlobalPlayerStatsQuery>
{
    public GetGlobalPlayerStatsValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
