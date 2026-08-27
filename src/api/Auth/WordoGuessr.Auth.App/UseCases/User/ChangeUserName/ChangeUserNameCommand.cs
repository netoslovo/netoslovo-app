using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.ChangeUserName;

public sealed record ChangeUserNameCommand(Guid PlayerId, string NewUserName)
    : ICommand<Result<ChangeUserNameError>>;
