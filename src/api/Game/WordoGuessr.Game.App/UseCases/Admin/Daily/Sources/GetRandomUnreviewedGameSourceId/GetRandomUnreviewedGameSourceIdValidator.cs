using FluentValidation;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetRandomUnreviewedGameSourceId;

internal sealed class GetRandomUnreviewedGameSourceIdValidator
    : AbstractValidator<GetRandomUnreviewedGameSourceIdQuery>
{
    public GetRandomUnreviewedGameSourceIdValidator()
    {
        RuleFor(x => x.DifficultyCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => Difficulty.TryFromCode(x, out _))
            .WithMessage(x => $"Unknown difficulty code - {x.DifficultyCode}");
    }
}
