using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Domain;

public sealed class GameSource : DomainEntity<long>
{
    public Word Word { get; } = null!;

    public GameSource(long id, Word word)
        : base(id)
    {
        Word = word;
    }
    private GameSource() { }

}
