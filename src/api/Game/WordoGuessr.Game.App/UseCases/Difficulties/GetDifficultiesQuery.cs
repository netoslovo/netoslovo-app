using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Difficulties;

public sealed class GetDifficultiesQuery : IQuery<IReadOnlyCollection<DifficultyDto>>
{
}
