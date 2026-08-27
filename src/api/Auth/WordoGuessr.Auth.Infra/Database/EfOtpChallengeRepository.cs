using Microsoft.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class EfOtpChallengeRepository : IOtpChallengeRepository
{
    private readonly AuthDbContext _dbContext;

    public EfOtpChallengeRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(OtpChallenge challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);

        _dbContext.Set<OtpChallenge>().Add(challenge);
    }

    public Task<OtpChallenge?> FindById(Guid id, CancellationToken ct = default)
    {
        return _dbContext.Set<OtpChallenge>()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
