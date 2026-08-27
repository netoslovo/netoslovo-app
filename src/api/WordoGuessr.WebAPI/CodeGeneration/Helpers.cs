using System.Reflection;

namespace WordoGuessr.WebAPI.CodeGeneration;

// https://wolverinefx.net/guide/codegen#handling-code-generation-with-wolverine-when-using-aspire-or-microsoft-extensions-apidescription-server
internal static class Helpers
{
    public const string CodeGenPlaceholderConnectionString =
        "Host=codegen;Database=codegen;Username=codegen;Password=codegen";

    public static bool IsRunningGeneration()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider" || Environment.GetCommandLineArgs().Contains("codegen");
    }
}