using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.RequestEmailOtp;

public sealed record RequestEmailOtpCommand(string Email)
    : ICommand<Result<Guid, RequestEmailOtpError>>;
