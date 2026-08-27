using FluentValidation;

namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetSourceByWordWithReview;

internal sealed class GetSourceByWordWithReviewValidator
    : AbstractValidator<GetSourceByWordWithReviewQuery>
{
    public GetSourceByWordWithReviewValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty();
    }
}