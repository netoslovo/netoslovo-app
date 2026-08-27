using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;

public sealed record ReviewDailyGameSourceCommand(long GameSourceId, bool Approved)
    : ICommand<Result<ReviewDailyGameSourceError>>;
