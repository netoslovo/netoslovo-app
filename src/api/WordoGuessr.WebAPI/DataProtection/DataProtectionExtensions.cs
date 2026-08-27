using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.WebAPI.CodeGeneration;

namespace WordoGuessr.WebAPI.DataProtection;

internal static class DataProtectionExtensions
{
    private const string RedisKey = "data-protection";
    public static IServiceCollection AddRedisConfiguredDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        Func<IDatabase> redisDatabaseFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var dataProtection = services
            .AddDataProtection()
            .SetApplicationName(applicationName);

        // TODO: https://github.com/dotnet/aspnetcore/issues/23033
        if (Helpers.IsRunningGeneration())
        {
            return services;
        }

        var certificatePath =
            configuration.GetRequiredValue(Const.DataProtectionCertPathConfigurationKey);

        var certificatePassword =
            configuration.GetRequiredValue(Const.DataProtectionCertPassConfigurationKey);

        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            certificatePassword,
            X509KeyStorageFlags.EphemeralKeySet);

        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException(
                "DataProtection certificate must contain a private key.");
        }

        dataProtection
            .PersistKeysToStackExchangeRedis(redisDatabaseFactory, RedisKey)
            .ProtectKeysWithCertificate(certificate);

        return services;
    }
}
