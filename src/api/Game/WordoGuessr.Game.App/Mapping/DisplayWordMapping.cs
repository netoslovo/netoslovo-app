using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DisplayWordMapping
{
    public static DisplayWordDto MapToDto(this DisplayWordView displayWord, DisplayWordHideReason? hideReason = null) =>
        new DisplayWordDto(displayWord.Cells?.Select(c => c.MapToDto()).ToArray(), hideReason?.MapToDto());

    public static DisplayWordCellDto MapToDto(this DisplayWordCellView displayWordCell) =>
        new DisplayWordCellDto(displayWordCell.Value, displayWordCell.Revealed);

    public static DisplayWordDtoHideReason MapToDto(this DisplayWordHideReason hideReason) =>
        hideReason switch
        {
            DisplayWordHideReason.HiddenForToday => DisplayWordDtoHideReason.HiddenForToday,
            _ => throw new ArgumentOutOfRangeException(nameof(hideReason), hideReason, "Unknown hide reason")
        };
}
