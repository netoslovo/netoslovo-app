using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.WebAPI.CodeGeneration;

namespace WordoGuessr.WebAPI.DataProtection;

internal static class DataProtectionExtensions
{
    public static IServiceCollection AddEfConfiguredDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string connectionString)
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

        services.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<DataProtectionDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<DataProtectionDbContext>(sp, connectionString));

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
            .PersistKeysToDbContext<DataProtectionDbContext>()
            .ProtectKeysWithCertificate(certificate);

        return services;
    }
}
