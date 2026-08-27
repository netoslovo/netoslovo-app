using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;

internal sealed class MakeGuessValidator : AbstractValidator<MakeGuessCommand>
{
    public MakeGuessValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.Word)
            .NotNull()
            .NotEmpty();
    }
}
