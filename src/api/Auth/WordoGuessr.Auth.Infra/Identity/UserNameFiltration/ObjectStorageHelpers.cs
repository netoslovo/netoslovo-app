namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal static class ObjectStorageHelpers
{
    public static string GetFullPathForKey(string objectKey, int version) =>
        $"auth/user-name-filter/v{version}/{objectKey}";
}
