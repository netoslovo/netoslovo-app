using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.User.RequestEmailOtp;

public sealed class RequestEmailOtpValidator : AbstractValidator<RequestEmailOtpCommand>
{
    public RequestEmailOtpValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
