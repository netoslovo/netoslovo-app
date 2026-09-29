using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetSharedGame;

internal sealed class GetSharedGameValidator : AbstractValidator<GetSharedGameQuery>
{
    public GetSharedGameValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty();

        RuleFor(x => x.ViewerPlayerId)
            .NotEmpty();
    }
}
