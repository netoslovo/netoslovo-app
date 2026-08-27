using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.User.ChangeUserName;

public sealed class ChangeUserNameValidator : AbstractValidator<ChangeUserNameCommand>
{
    public ChangeUserNameValidator()
    {
        RuleFor(command => command.NewUserName)
            .NotEmpty();

        RuleFor(x => x.PlayerId)
            .NotEmpty();
    }
}
