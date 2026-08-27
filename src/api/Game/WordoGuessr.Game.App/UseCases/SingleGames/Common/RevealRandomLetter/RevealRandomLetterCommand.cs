using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;

public sealed record RevealRandomLetterCommand(Guid PlayerId, Guid GameId)
    : ICommand<Result<TextHintDto, RevealRandomLetterError>>;
