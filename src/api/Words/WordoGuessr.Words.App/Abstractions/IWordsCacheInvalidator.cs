namespace WordoGuessr.Words.App.Abstractions;

public interface IWordsCacheInvalidator
{
    ValueTask RemoveVersion(int version, CancellationToken ct = default);
    ValueTask RemoveVersions(int upToVersion, CancellationToken ct = default);
    ValueTask Clear(CancellationToken ct = default);
}