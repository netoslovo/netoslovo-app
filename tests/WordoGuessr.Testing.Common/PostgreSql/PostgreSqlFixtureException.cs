namespace WordoGuessr.Testing.Common.PostgreSql;

internal class PostgreSqlFixtureException : Exception
{
    public PostgreSqlFixtureException(string message)
        : base(message)
    {

    }

    public PostgreSqlFixtureException(string message, Exception inner)
        : base(message, inner)
    {

    }
}
