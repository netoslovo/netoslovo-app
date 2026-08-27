using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.User.SaveOneTimeUserNoticeView;

internal sealed class SaveOneTimeUserNoticeViewValidator
    : AbstractValidator<SaveOneTimeUserNoticeViewCommand>
{
    public SaveOneTimeUserNoticeViewValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Code)
            .NotEmpty();
    }
}