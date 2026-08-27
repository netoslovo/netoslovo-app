using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;

internal sealed class RevealRandomLetterValidator : AbstractValidator<RevealRandomLetterCommand>
{
    public RevealRandomLetterValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
