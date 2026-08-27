using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;

public sealed record RevealHalfwayWordHintCommand(Guid PlayerId, Guid GameId)
    : ICommand<Result<GuessHintDto, RevealHalfwayWordHintError>>;
