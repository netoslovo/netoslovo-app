using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class WordValueConverter : ValueConverter<Word, string>
{
    public WordValueConverter() :
        base(word => word.Text,
             str => Word.Create(str))
    { }
}
