using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;

public sealed record RevealWordLengthCommand(Guid PlayerId, Guid GameId)
    : ICommand<Result<TextHintDto, RevealWordLengthError>>;
