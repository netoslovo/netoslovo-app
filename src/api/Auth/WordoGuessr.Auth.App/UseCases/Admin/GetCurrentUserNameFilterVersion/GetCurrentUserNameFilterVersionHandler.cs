using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.App.UseCases.Admin.GetCurrentUserNameFilterVersion;

public sealed class GetCurrentUserNameFilterVersionHandler
    : IQueryHandler<GetCurrentUserNameFilterVersionQuery, int>
{
    private readonly IUserNameFilterVersionStore _userNameFilterVersionStore;

    public GetCurrentUserNameFilterVersionHandler(
        IUserNameFilterVersionStore userNameFilterVersionStore)
    {
        _userNameFilterVersionStore = userNameFilterVersionStore
            ?? throw new ArgumentNullException(nameof(userNameFilterVersionStore));
    }

    public Task<int> Handle(GetCurrentUserNameFilterVersionQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _userNameFilterVersionStore.GetLatestVersion(ct);
    }
}
