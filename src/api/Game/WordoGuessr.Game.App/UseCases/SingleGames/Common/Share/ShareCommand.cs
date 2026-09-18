using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Share;

public sealed record ShareCommand(
    Guid PlayerId,
    Guid GameId,
    bool ShowGuessWords)
    : ICommand<Result<Guid, ShareError>>;
