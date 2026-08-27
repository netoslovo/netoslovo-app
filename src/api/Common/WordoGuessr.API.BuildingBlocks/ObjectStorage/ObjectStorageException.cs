namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public class ObjectStorageException : Exception
{
    public ObjectStorageException(string message)
        : base(message)
    {
    }

    public ObjectStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
