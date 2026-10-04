using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DisplayWordMapping
{
    public static DisplayWordDto MapToDto(this DisplayWord displayWord) =>
        new DisplayWordDto(displayWord.Cells?.Select(c => c.MapToDto()).ToArray());

    public static DisplayWordCellDto MapToDto(this DisplayWordCell displayWordCell) =>
        new DisplayWordCellDto(displayWordCell.Value, displayWordCell.Revealed);
}
