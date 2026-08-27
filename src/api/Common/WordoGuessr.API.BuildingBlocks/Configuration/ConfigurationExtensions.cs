using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace WordoGuessr.API.BuildingBlocks.Configuration;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration.GetValue<string>(key);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Configuration value '{key}' is required.");
    }

    public static TOptions GetValidatedOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, INamedOptions
    {
        var options = configuration
            .GetRequiredSection(TOptions.Name)
            .Get<TOptions>(options =>
            {
                options.BindNonPublicProperties = true;
            })
        ?? throw new InvalidOperationException($"Couldn't get {typeof(TOptions)} object from {TOptions.Name} section");

        Validator.ValidateObject(
            options,
            new ValidationContext(options),
            validateAllProperties: true);

        return options;
    }

    public static TOptions? GetOptionalValidatedOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, INamedOptions
    {
        var section = configuration.GetSection(TOptions.Name);

        if (!section.Exists())
        {
            return null;
        }

        var options = section.Get<TOptions>();

        if (options is null)
        {
            return null;
        }

        Validator.ValidateObject(
            options,
            new ValidationContext(options),
            validateAllProperties: true);

        return options;
    }

    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");
    }
}
