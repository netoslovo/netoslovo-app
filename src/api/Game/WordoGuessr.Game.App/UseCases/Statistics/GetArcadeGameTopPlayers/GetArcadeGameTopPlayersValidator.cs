using FluentValidation;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetArcadeGameTopPlayers;

internal sealed class GetArcadeGameTopPlayersValidator
    : AbstractValidator<GetArcadeGameTopPlayersQuery>
{
    private const int MaxTopN = 10;

    public GetArcadeGameTopPlayersValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty();

        RuleFor(x => x.DifficultyCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => Difficulty.TryFromCode(x, out _))
            .WithMessage(x => $"Unknown difficulty code - {x.DifficultyCode}");

        RuleFor(x => x.TopN)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTopN);
    }
}