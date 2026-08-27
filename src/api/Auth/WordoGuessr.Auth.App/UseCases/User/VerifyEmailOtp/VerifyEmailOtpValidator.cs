using FluentValidation;

namespace WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;

public sealed class VerifyEmailOtpValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpValidator()
    {
        RuleFor(command => command.ChallengeId)
            .NotEmpty();

        RuleFor(command => command.Code)
            .NotEmpty();
    }
}
