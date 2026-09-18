using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.GetSharedGame;

internal sealed class GetSharedGameHandler : IQueryHandler<GetSharedGameQuery, Result<GameDto, GetSharedGameError>>
{
    public Task<Result<GameDto, GetSharedGameError>> Handle(
        GetSharedGameQuery query,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
