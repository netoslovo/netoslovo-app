using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class DifficultyValueConverter : ValueConverter<Difficulty, string>
{
    public DifficultyValueConverter()
        : base(
            difficulty => difficulty.Code,
            code => Difficulty.FromCode(code))
    { }
}
