using WordoGuessr.Common.App.Exceptions;

namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public class ObjectStorageTransientException : Exception, ITransientException
{
    public ObjectStorageTransientException(string message)
        : base(message)
    {
    }

    public ObjectStorageTransientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}