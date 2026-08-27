using FluentValidation;

namespace WordoGuessr.Sagas.App.UseCases.GetWordsVersionUploadHistory;

internal sealed class GetWordsVersionUploadHistoryValidator
    : AbstractValidator<GetWordsVersionUploadHistoryQuery>
{
    private const int MaxTake = 50;

    public GetWordsVersionUploadHistoryValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake);

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From is not null && x.To is not null);

        RuleFor(x => x.WordsVersion)
            .GreaterThan(0)
            .When(x => x.WordsVersion is not null);
    }
}
