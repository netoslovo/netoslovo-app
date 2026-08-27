using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.Admin.PublishNewUserNameFilterVersion;

internal sealed class PublishNewUserNameFilterVersionValidator
    : AbstractValidator<PublishNewUserNameFilterVersionCommand>
{
    public PublishNewUserNameFilterVersionValidator()
    {
        RuleFor(x => x.Version)
            .GreaterThan(0);
    }
}
