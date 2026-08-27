using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetArcadeGameTopPlayers;

public sealed record GetArcadeGameTopPlayersQuery(Guid PlayerId, string DifficultyCode, int TopN)
    : IQuery<ArcadeGameTopPlayersDto>;