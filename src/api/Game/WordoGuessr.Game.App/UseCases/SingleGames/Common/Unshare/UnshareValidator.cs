using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Unshare;

internal sealed class UnshareValidator : AbstractValidator<UnshareCommand>
{
    public UnshareValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
