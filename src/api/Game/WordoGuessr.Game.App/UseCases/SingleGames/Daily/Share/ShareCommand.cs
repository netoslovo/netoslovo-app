using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.Share;

public sealed record ShareCommand(
    Guid PlayerId,
    Guid GameId)
    : ICommand<Result<Guid, ShareError>>;
