using Microsoft.EntityFrameworkCore;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Common.App.Exceptions.Persistence;

namespace WordoGuessr.Auth.Infra.Database;

internal sealed class EfUserNoticeViewStore : IUserNoticeViewStore
{
    private readonly AuthDbContext _dbContext;

    public EfUserNoticeViewStore(AuthDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<UserNoticeView?> FindUserNoticeView(Guid userId, string code, CancellationToken ct = default)
    {
        return _dbContext.Set<UserNoticeView>()
            .FirstOrDefaultAsync(view => view.UserId == userId && view.NoticeCode == code, ct);
    }

    public async Task AddOneTimeView(OneTimeUserNoticeView view, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        _dbContext.Set<OneTimeUserNoticeView>().Add(view);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            _dbContext.Entry(view).State = EntityState.Detached;
        }
    }

    public async Task AddOrUpdateRecurringView(RecurringUserNoticeView view, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(view);

        var existingView = await _dbContext.Set<RecurringUserNoticeView>().SingleOrDefaultAsync(
            v => v.UserId == view.UserId && v.NoticeCode == view.NoticeCode,
            ct);

        if (existingView is null)
        {
            _dbContext.Set<RecurringUserNoticeView>().Add(view);

            try
            {
                await _dbContext.SaveChangesAsync(ct);
                return;
            }
            catch (UniqueConstraintViolationException)
            {
                _dbContext.Entry(view).State = EntityState.Detached;

                existingView = await _dbContext.Set<RecurringUserNoticeView>().SingleAsync(
                    v => v.UserId == view.UserId && v.NoticeCode == view.NoticeCode,
                    ct);
            }
        }

        if (view.DoNotShowAgain)
        {
            existingView.Disable();
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
