using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal sealed class DisplayWordDtoBuilder
{
    private readonly DailyGameMetadataExtractor _dailyGameMetadataExtractor;

    public DisplayWordDtoBuilder(DailyGameMetadataExtractor dailyGameMetadataExtractor)
    {
        _dailyGameMetadataExtractor = dailyGameMetadataExtractor ?? throw new ArgumentNullException(nameof(dailyGameMetadataExtractor));
    }

    public DisplayWordDto Build(SingleGame game)
    {
        var policy = new DisplayWordVisibilityPolicy(
            game.StateCode,
            _dailyGameMetadataExtractor.ExtractOrDefault(game));

        var displayWord = policy.ShouldShowRevealedWord(out var hideReason)
            ? game.GetRevealedDisplayWord().MapToDto()
            : game.GetCurrentDisplayWord().MapToDto(hideReason);

        return displayWord;
    }
}
