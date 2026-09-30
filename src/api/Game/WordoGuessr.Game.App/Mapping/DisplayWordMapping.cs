using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DisplayWordMapping
{
    public static DisplayWordDto MapToDto(this DisplayWordView displayWord, DisplayWordUnavailableReason? hideReason = null) =>
        new DisplayWordDto(displayWord.Cells?.Select(c => c.MapToDto()).ToArray(), hideReason?.MapToDto());

    public static DisplayWordCellDto MapToDto(this DisplayWordCellView displayWordCell) =>
        new DisplayWordCellDto(displayWordCell.Value, displayWordCell.Revealed);

    public static DisplayWordHideReasonDto MapToDto(this DisplayWordUnavailableReason hideReason) =>
        hideReason switch
        {
            DisplayWordUnavailableReason.HiddenForToday => DisplayWordHideReasonDto.HiddenForToday,
            _ => throw new ArgumentOutOfRangeException(nameof(hideReason), hideReason, "Unknown hide reason")
        };
}
