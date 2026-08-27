using Shouldly;

namespace WordoGuessr.Testing.Common;

public static class PostgreSqlDateTimeOffsetAssertions
{
    private static readonly TimeSpan PostgreSqlTimestampTolerance = TimeSpan.FromTicks(10);

    public static void ShouldBeAtPostgreSqlPrecision(
        this DateTimeOffset actual,
        DateTimeOffset expected,
        string? customMessage = null)
    {
        actual.ShouldBe(expected, PostgreSqlTimestampTolerance, customMessage);
    }
}
