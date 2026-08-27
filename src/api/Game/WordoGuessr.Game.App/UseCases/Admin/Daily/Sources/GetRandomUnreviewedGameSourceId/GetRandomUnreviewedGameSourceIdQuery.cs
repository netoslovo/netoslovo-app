using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetRandomUnreviewedGameSourceId;

public sealed record GetRandomUnreviewedGameSourceIdQuery(string DifficultyCode)
    : IQuery<long?>;
