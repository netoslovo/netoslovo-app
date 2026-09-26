using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.Unshare;

public sealed record UnshareCommand(Guid PlayerId, Guid GameId) : ICommand;
