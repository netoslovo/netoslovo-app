using WordoGuessr.API.BuildingBlocks.CQRS;

namespace WordoGuessr.Auth.App.UseCases.Admin.GetCurrentUserNameFilterVersion;

public sealed record GetCurrentUserNameFilterVersionQuery : IQuery<int>;
