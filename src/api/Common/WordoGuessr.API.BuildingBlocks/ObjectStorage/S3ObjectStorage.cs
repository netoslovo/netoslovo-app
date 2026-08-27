using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net.Http;

namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly S3ObjectStorageOptions _options;

    public S3ObjectStorage(
        IAmazonS3 s3,
        IOptions<S3ObjectStorageOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task PutAsync(
        string objectKey,
        Stream content,
        ObjectUploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ValidateObjectKey(objectKey);

        ArgumentNullException.ThrowIfNull(content);

        try
        {
            await _s3.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = objectKey,
                    InputStream = content,
                    ContentType = options?.ContentType,
                },
                cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.Retryable is not null)
        {
            throw new ObjectStorageTransientException($"Failed to put object '{objectKey}'.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ObjectStorageTransientException($"Failed to put object '{objectKey}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new ObjectStorageException($"Failed to put object '{objectKey}'.", ex);
        }
    }

    public async Task<ObjectDownload> GetAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        ValidateObjectKey(objectKey);

        try
        {
            var response = await _s3.GetObjectAsync(
                _options.BucketName,
                objectKey,
                cancellationToken);

            return new ObjectDownload(
                response.ResponseStream,
                response.Headers.ContentType,
                response.Headers.ContentLength);
        }
        catch (AmazonS3Exception ex) when (IsNotFound(ex))
        {
            throw new ObjectNotFoundException(objectKey);
        }
        catch (AmazonS3Exception ex) when (ex.Retryable is not null)
        {
            throw new ObjectStorageTransientException($"Failed to get object '{objectKey}'.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ObjectStorageTransientException($"Failed to get object '{objectKey}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new ObjectStorageException($"Failed to get object '{objectKey}'.", ex);
        }
    }

    public async Task<bool> ExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        ValidateObjectKey(objectKey);

        try
        {
            await _s3.GetObjectMetadataAsync(
                _options.BucketName,
                objectKey,
                cancellationToken);

            return true;
        }
        catch (AmazonS3Exception ex) when (IsNotFound(ex))
        {
            return false;
        }
        catch (AmazonS3Exception ex) when (ex.Retryable is not null)
        {
            throw new ObjectStorageTransientException($"Failed to check object '{objectKey}'.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ObjectStorageTransientException($"Failed to check object '{objectKey}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new ObjectStorageException($"Failed to check object '{objectKey}'.", ex);
        }
    }

    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        ValidateObjectKey(objectKey);

        try
        {
            await _s3.DeleteObjectAsync(
                _options.BucketName,
                objectKey,
                cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.Retryable is not null)
        {
            throw new ObjectStorageTransientException($"Failed to delete object '{objectKey}'.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ObjectStorageTransientException($"Failed to delete object '{objectKey}'.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new ObjectStorageException($"Failed to delete object '{objectKey}'.", ex);
        }
    }

    private static void ValidateObjectKey(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException("Object key must not be empty.", nameof(objectKey));
        }
    }

    private static bool IsNotFound(AmazonS3Exception exception)
    {
        return exception.StatusCode == System.Net.HttpStatusCode.NotFound
            || exception.ErrorCode == "NoSuchKey";
    }
}
