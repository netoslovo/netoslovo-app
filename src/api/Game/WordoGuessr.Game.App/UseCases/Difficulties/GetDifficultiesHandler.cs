using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Difficulties;

internal sealed class GetDifficultiesHandler : IQueryHandler<GetDifficultiesQuery, IReadOnlyCollection<DifficultyDto>>
{
    public Task<IReadOnlyCollection<DifficultyDto>> Handle(GetDifficultiesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyCollection<DifficultyDto> difficulties = Difficulty.All.MapToDto();
        return Task.FromResult(difficulties);
    }
}
