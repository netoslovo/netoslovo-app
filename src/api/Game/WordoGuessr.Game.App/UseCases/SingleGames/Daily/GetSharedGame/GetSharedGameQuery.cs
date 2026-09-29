using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetSharedGame;

public sealed record GetSharedGameQuery(Guid PublicId, Guid ViewerPlayerId)
    : IQuery<Result<SharedDailyGameDto, GetSharedGameError>>;
