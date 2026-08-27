using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WordoGuessr.API.BuildingBlocks.DbContextCommon;

public sealed class PersistenceExceptionMappingInterceptor : SaveChangesInterceptor
{
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        throw PersistenceExceptionsMapper.MapToPersistenceException(eventData.Exception);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        throw PersistenceExceptionsMapper.MapToPersistenceException(eventData.Exception);
    }
}