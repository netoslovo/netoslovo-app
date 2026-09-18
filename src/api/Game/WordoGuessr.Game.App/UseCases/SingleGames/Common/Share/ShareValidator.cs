using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Share;

internal sealed class ShareValidator : AbstractValidator<ShareCommand>
{
    public ShareValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
