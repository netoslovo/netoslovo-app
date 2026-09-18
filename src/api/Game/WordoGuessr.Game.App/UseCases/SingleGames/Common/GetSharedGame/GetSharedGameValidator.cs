using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.GetSharedGame;

internal sealed class GetSharedGameValidator : AbstractValidator<GetSharedGameQuery>
{
    public GetSharedGameValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty();
    }
}
