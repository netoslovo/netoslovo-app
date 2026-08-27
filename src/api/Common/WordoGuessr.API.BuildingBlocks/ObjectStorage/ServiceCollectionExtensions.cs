using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using Amazon.S3;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddS3ObjectStorage(
        this IServiceCollection services)
    {
        services.AddOptions<S3ObjectStorageOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindNamedConfiguration();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3ObjectStorageOptions>>().Value;
            var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

            return new AmazonS3Client(
                credentials,
                new AmazonS3Config
                {
                    ServiceURL = options.ServiceUrl,
                    ForcePathStyle = options.ForcePathStyle,
                    UseHttp = options.UseHttp,
                    AuthenticationRegion = options.Region
                });
        });

        services.AddSingleton<IObjectStorage, S3ObjectStorage>();

        return services;
    }


}
