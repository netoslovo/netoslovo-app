using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetShare;

internal sealed class GetShareValidator : AbstractValidator<GetShareQuery>
{
    public GetShareValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
