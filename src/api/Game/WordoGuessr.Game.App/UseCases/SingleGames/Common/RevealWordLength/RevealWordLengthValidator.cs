using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;

internal sealed class RevealWordLengthValidator : AbstractValidator<RevealWordLengthCommand>
{
    public RevealWordLengthValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
