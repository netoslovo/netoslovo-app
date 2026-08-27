namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public sealed class ObjectNotFoundException : ObjectStorageException
{
    public ObjectNotFoundException(string objectKey)
        : base($"Object '{objectKey}' was not found.")
    {
        ObjectKey = objectKey;
    }

    public string ObjectKey { get; }
}
