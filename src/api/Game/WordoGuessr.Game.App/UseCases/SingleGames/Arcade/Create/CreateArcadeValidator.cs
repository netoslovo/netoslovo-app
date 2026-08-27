using FluentValidation;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.Create;

internal sealed class CreateArcadeValidator : AbstractValidator<CreateArcadeCommand>
{
    public CreateArcadeValidator()
    {
        RuleFor(x => x.DifficultyCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => Difficulty.TryFromCode(x, out _))
            .WithMessage(x => $"Unknown difficulty code - {x.DifficultyCode}");
    }
}
