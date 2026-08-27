using System.ComponentModel.DataAnnotations;
using OpenTelemetry.Exporter;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.WebAPI.OpenTelemetry;

internal sealed class OpenTelemetryOptions : INamedOptions, IValidatableObject
{
    public static string Name => "OpenTelemetry";

    [Required]
    public required string OtlpEndpoint { get; init; }

    [Required]
    public required OtlpExportProtocol OtlpProtocol { get; init; }

    public TimeSpan ExportInterval { get; init; }
    public string? ServiceInstanceId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(OtlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            yield return new ValidationResult(
                $"{nameof(OtlpEndpoint)} must be an absolute URI.",
                [nameof(OtlpEndpoint)]);

            yield break;
        }

        if (endpoint.Scheme != "http" && endpoint.Scheme != "https")
        {
            yield return new ValidationResult(
                $"{nameof(OtlpEndpoint)} must use http or https scheme.",
                [nameof(OtlpEndpoint)]);
        }

        if (!Enum.IsDefined(OtlpProtocol))
        {
            yield return new ValidationResult(
                $"{nameof(OtlpProtocol)} must be one of: {string.Join(", ", Enum.GetNames<OtlpExportProtocol>())}.",
                [nameof(OtlpProtocol)]);
        }

        if (ExportInterval.TotalMilliseconds < 1 || ExportInterval.TotalMilliseconds > int.MaxValue)
        {
            yield return new ValidationResult(
                $"{nameof(ExportInterval)} must be between 1 millisecond and {int.MaxValue} milliseconds.",
                [nameof(ExportInterval)]);
        }
    }

    public Uri GetEndpoint() => new Uri(OtlpEndpoint);
}
