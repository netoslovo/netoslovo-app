using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Arcade.Create;

public sealed record CreateArcadeCommand(Guid PlayerId, string DifficultyCode) : ICommand<Result<GameDto, CreateArcadeError>>;
