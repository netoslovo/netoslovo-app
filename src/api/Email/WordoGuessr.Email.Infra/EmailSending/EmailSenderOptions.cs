using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Email.Infra.EmailSending;

public sealed class EmailSenderOptions : INamedOptions, IValidatableObject
{
    public static string Name => "Email:Sender";

    [Required]
    public required string SmtpServer { get; set; }

    [Range(1, 65535)]
    public int SmtpPort { get; set; }

    public string? SmtpLogin { get; set; }

    public string? SmtpPassword { get; set; }

    [Required]
    public required string FromBaseAddress { get; set; }

    [Required]
    public required string FromDomain { get; set; }

    [Required]
    public required string FromDisplayName { get; set; }

    public TimeSpan SendingTimeout { get; set; }

    public bool UseSSL { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SendingTimeout <= TimeSpan.Zero)
        {
            yield return new ValidationResult(
                $"{nameof(SendingTimeout)} must be greater than zero.",
                [nameof(SendingTimeout)]);
        }
    }
}
