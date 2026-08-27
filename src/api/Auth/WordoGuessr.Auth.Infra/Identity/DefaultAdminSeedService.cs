using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class DefaultAdminSeedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DefaultAdminSeedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DefaultAdminOptions>>().Value;
        var userStore = scope.ServiceProvider.GetRequiredService<IUserService>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IAuthUnitOfWork>();

        using var transactionScope = await unitOfWork.BeginTransactionScope(ct);
        foreach (var email in options.Emails)
        {
            var emailAddress = EmailAddress.Create(email);
            var user = await GetOrCreateUser(userStore, timeProvider, email, emailAddress);

            var assignRoleResult = await userStore.AssignRoleToUser(user.Id, AppRoles.Admin);
            if (!assignRoleResult.IsSuccess)
            {
                throw CreateSeedException(email, assignRoleResult.Error);
            }
        }

        await transactionScope.Commit(ct);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private static UserServiceException CreateSeedException(string email, UserErrorCode error)
        => new UserServiceException([new UserServiceError(error.ToString(), $"Could not seed admin user '{email}'.")]);

    private static async Task<ApplicationUser> GetOrCreateUser(
        IUserService userStore,
        TimeProvider timeProvider,
        string email,
        EmailAddress emailAddress)
    {
        var user = await userStore.FindByEmail(emailAddress);
        if (user is not null)
        {
            return user;
        }

        var now = timeProvider.GetUtcNow();
        var id = Guid.CreateVersion7(now);
        var createResult = await userStore.CreateConfirmedEmailUser(id, emailAddress);
        if (createResult.IsSuccess)
        {
            return createResult.Value;
        }

        if (createResult.Error == UserErrorCode.DuplicateEmail)
        {
            user = await userStore.FindByEmail(emailAddress);
            if (user is not null)
            {
                return user;
            }
        }

        throw CreateSeedException(email, createResult.Error);
    }
}
