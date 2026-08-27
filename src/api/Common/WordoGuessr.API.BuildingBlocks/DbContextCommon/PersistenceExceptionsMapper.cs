using Microsoft.EntityFrameworkCore;
using Npgsql;
using WordoGuessr.Common.App.Exceptions.Persistence;

namespace WordoGuessr.API.BuildingBlocks.DbContextCommon;

public static class PersistenceExceptionsMapper
{
    public static PersistenceException MapToPersistenceException(Exception exception)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            return new ConcurrencyConflictException(exception);
        }

        var postgresException = FindPostgresException(exception);
        if (postgresException is not null)
        {
            return postgresException.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation =>
                    new UniqueConstraintViolationException(exception, postgresException.ConstraintName),

                PostgresErrorCodes.ForeignKeyViolation =>
                    new ForeignKeyConstraintViolationException(exception, postgresException.ConstraintName),

                PostgresErrorCodes.NotNullViolation or
                PostgresErrorCodes.CheckViolation =>
                    new DataConstraintViolationException(exception, postgresException.ConstraintName),

                _ when postgresException.IsTransient =>
                    new TransientPersistenceException(exception),

                _ => new UnknownPersistenceException(exception)
            };
        }

        var npgsqlException = FindNpgsqlException(exception);
        if (npgsqlException is not null)
        {
            return npgsqlException.IsTransient
                ? new TransientPersistenceException(exception)
                : new UnknownPersistenceException(exception);
        }

        return new UnknownPersistenceException(exception);
    }

    private static NpgsqlException? FindNpgsqlException(Exception exception)
    {
        var current = exception;
        while (current is not null)
        {
            if (current is NpgsqlException postgresException)
            {
                return postgresException;
            }

            current = current.InnerException;
        }

        return null;
    }


    private static PostgresException? FindPostgresException(Exception exception)
    {
        var current = exception;
        while (current is not null)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            current = current.InnerException;
        }

        return null;
    }
}