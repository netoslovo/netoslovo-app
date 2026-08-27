using Wolverine.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class EfAuthUnitOfWork : IAuthUnitOfWork
{
    public IOutbox Outbox { get; }
    public IOtpChallengeRepository OtpChallenges { get; }
    public IUserService Users { get; }

    private readonly IDbContextOutbox<AuthDbContext> _dbContextOutbox;

    public EfAuthUnitOfWork(
        IDbContextOutbox<AuthDbContext> dbContextOutbox,
        IOtpChallengeRepository otpChallengeRepository,
        IUserService userRepository)
    {
        _dbContextOutbox = dbContextOutbox
            ?? throw new ArgumentNullException(nameof(dbContextOutbox));

        Outbox = new OutboxWrapper(_dbContextOutbox);

        OtpChallenges = otpChallengeRepository
            ?? throw new ArgumentNullException(nameof(otpChallengeRepository));

        Users = userRepository
            ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<IAuthTransactionScope> BeginTransactionScope(CancellationToken ct = default)
    {
        var innerTransaction = await _dbContextOutbox.DbContext.Database.BeginTransactionAsync(ct);
        return new EfAuthTransactionScope(innerTransaction, _dbContextOutbox);
    }
}
