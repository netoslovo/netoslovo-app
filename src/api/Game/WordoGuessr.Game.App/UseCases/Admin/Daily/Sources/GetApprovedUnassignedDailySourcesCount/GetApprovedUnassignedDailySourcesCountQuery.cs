using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetApprovedUnassignedDailySourcesCount;

public sealed record GetApprovedUnassignedDailySourcesCountQuery()
    : IQuery<int>;
