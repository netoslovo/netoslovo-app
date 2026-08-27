using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;

internal sealed class RevealHalfwayWordValidator
    : AbstractValidator<RevealHalfwayWordHintCommand>
{
    public RevealHalfwayWordValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
