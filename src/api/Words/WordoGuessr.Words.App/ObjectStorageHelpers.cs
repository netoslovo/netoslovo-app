namespace WordoGuessr.Words.App;

internal static class ObjectStorageHelpers
{
    public static string GetFullPathForKey(int version, string objectKey) =>
        $"words/v{version}/{objectKey}";
}
