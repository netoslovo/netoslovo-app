using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.GetSharedGame;

public sealed record GetSharedGameQuery(Guid PublicId)
    : IQuery<Result<GameDto, GetSharedGameError>>;
