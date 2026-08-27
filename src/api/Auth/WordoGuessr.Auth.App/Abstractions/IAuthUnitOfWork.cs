namespace WordoGuessr.Auth.App.Abstractions;

public interface IAuthUnitOfWork
{
    IOtpChallengeRepository OtpChallenges { get; }
    IUserService Users { get; }
    IOutbox Outbox { get; }
    Task<IAuthTransactionScope> BeginTransactionScope(CancellationToken ct = default);
}
