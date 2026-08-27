using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Email.Infra.Database.EntityConfigurations;

internal sealed class EmailHtmlBodyValueConverter : ValueConverter<EmailHtmlBody, string>
{
    public EmailHtmlBodyValueConverter()
        : base(body => body.Value,
               value => EmailHtmlBody.Create(value))
    {
    }
}
