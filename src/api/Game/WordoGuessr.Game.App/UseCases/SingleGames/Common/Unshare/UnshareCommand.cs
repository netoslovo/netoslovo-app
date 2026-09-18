using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.Unshare;

public sealed record UnshareCommand(Guid PlayerId, Guid GameId) : ICommand;
