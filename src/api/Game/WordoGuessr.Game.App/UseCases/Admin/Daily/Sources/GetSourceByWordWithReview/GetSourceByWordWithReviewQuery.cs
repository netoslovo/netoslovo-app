using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetSourceByWordWithReview;

public sealed record GetSourceByWordWithReviewQuery(string Word)
    : IQuery<Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>>;