using FluentValidation;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploads;

internal sealed class GetWordsVersionUploadsValidator
    : AbstractValidator<GetWordsVersionUploadsQuery>
{
    private const int MaxTake = 50;

    public GetWordsVersionUploadsValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake);
    }
}
