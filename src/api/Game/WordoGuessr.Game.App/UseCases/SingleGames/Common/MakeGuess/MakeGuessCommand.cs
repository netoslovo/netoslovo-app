using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;

public sealed record MakeGuessCommand(Guid PlayerId, Guid GameId, string Word)
    : ICommand<Result<GuessOutcomeDto, MakeGuessError>>;
