namespace WordoGuessr.WebAPI;

internal static class Const
{
    public const string JwtCookieName = "wg_data";
    public const string RefreshCookieName = "wg_refresh";
    public const string UserInfoCookieName = "wg_user";
    public const string GuestIdClaimName = "guestId";
    public const string PgConnectionStringName = "PostgreSql";
    public const string RedisConnectionStringName = "Redis";
    public const string DataProtectionCertPathConfigurationKey = "DataProtection:CertificatePath";
    public const string DataProtectionCertPassConfigurationKey = "DataProtection:CertificatePassword";
    public const string WolverineSchemaName = "wolverine";
}
