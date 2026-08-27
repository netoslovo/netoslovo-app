using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Email.Infra.Database.EntityConfigurations;

internal sealed class EmailSubjectValueConverter : ValueConverter<EmailSubject, string>
{
    public EmailSubjectValueConverter()
        : base(subject => subject.Value,
               value => EmailSubject.Create(value))
    {
    }
}
