namespace WordoGuessr.API.BuildingBlocks.ObjectStorage;

public sealed class ObjectDownload : IDisposable, IAsyncDisposable
{
    public Stream Content { get; }

    public string? ContentType { get; }

    public long? ContentLength { get; }

    public ObjectDownload(
        Stream content,
        string? contentType,
        long? contentLength)
    {
        Content = content;
        ContentType = contentType;
        ContentLength = contentLength;
    }

    public void Dispose() => Content.Dispose();

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
