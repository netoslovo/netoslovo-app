using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.VerifyEmailOtp;

public sealed record VerifyEmailOtpCommand(Guid ChallengeId, string Code)
    : ICommand<Result<VerifyEmailOtpError>>;
