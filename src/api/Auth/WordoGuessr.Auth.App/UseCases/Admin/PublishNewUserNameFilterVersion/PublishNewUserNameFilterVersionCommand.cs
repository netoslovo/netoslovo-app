using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Auth.App.UseCases.Admin.PublishNewUserNameFilterVersion;

public sealed record PublishNewUserNameFilterVersionCommand(int Version) : ICommand;
