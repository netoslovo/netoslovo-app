namespace WordoGuessr.Game.Dto;

public sealed record DisplayWordDto(IReadOnlyCollection<DisplayWordCellDto>? Cells, DisplayWordHideReasonDto? HideReason = null);

public sealed record DisplayWordCellDto(char? Value, bool Revealed);

public enum DisplayWordHideReasonDto
{
    HiddenForToday
}