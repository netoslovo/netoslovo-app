using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetHistory;

internal sealed class GetDailyGamesHistoryValidator
    : AbstractValidator<GetDailyGamesHistoryQuery>
{
    private const int MaxTake = 50;

    public GetDailyGamesHistoryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0);

        RuleFor(x => x.Take)
            .LessThanOrEqualTo(MaxTake);

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}