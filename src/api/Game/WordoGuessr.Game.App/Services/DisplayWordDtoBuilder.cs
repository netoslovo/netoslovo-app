using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Services;

internal sealed class DisplayWordDtoBuilder
{
    private readonly TimeProvider _timeProvider;

    public DisplayWordDtoBuilder(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DisplayWordDto Build(SingleGame game)
    {
        var today = DailyGameClock.GetDateOnly(_timeProvider.GetUtcNow());

        var displayWord = game.ShouldShowRevealedWord(today, out var hideReason)
            ? game.GetRevealedDisplayWord().MapToDto()
            : game.GetCurrentDisplayWord().MapToDto(hideReason);

        return displayWord;
    }
}