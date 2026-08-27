using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public sealed class S3ObjectStorageOptions : INamedOptions
{
    public static string Name => "ObjectStorage:S3";

    [Required]
    public string ServiceUrl { get; init; } = string.Empty;

    [Required]
    public string AccessKey { get; init; } = string.Empty;

    [Required]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string BucketName { get; init; } = string.Empty;

    public bool ForcePathStyle { get; init; } = true;

    public bool UseHttp { get; init; } = true;

    [Required]
    public string Region { get; init; } = string.Empty;
}
