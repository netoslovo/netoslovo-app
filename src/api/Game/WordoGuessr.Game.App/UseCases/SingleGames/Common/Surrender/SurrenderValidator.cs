using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Surrender;

internal sealed class SurrenderValidator : AbstractValidator<SurrenderCommand>
{
    public SurrenderValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
