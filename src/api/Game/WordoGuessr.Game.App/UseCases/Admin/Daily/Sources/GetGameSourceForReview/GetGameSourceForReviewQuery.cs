using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetGameSourceForReview;

public sealed record GetGameSourceForReviewQuery(long GameSourceId, int ClosestWordsCount)
    : IQuery<Result<UnreviewedSourceDto, GetGameSourceForReviewError>>;
