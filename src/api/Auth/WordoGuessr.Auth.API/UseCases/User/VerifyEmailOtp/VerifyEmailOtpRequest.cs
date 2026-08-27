namespace WordoGuessr.Auth.API.UseCases.User.VerifyEmailOtp;

internal sealed record VerifyEmailOtpRequest(Guid ChallengeId, string Code);
