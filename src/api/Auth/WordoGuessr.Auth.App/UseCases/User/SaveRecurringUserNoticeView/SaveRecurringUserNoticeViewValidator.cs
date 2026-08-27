using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.User.SaveRecurringUserNoticeView;

internal sealed class SaveRecurringUserNoticeViewValidator
    : AbstractValidator<SaveRecurringUserNoticeViewCommand>
{
    public SaveRecurringUserNoticeViewValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Code)
            .NotEmpty();
    }
}