using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;

internal sealed class GetArcadeGamesHistoryValidator
    : AbstractValidator<GetArcadeGamesHistoryQuery>
{
    private const int MaxTake = 50;

    public GetArcadeGamesHistoryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake);

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}