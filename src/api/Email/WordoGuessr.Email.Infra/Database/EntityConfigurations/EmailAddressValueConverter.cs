using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Email.Infra.Database.EntityConfigurations;

internal sealed class EmailAddressValueConverter : ValueConverter<EmailAddress, string>
{
    public EmailAddressValueConverter()
        : base(address => address.Value,
               value => EmailAddress.Create(value))
    {
    }
}
