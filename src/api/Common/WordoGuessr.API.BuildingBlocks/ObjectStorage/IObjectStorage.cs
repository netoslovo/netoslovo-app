namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public interface IObjectStorage
{
    Task PutAsync(
        string objectKey,
        Stream content,
        ObjectUploadOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<ObjectDownload> GetAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}
