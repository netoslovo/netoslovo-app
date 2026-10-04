namespace WordoGuessr.Game.Dto;

public sealed record DisplayWordDto(IReadOnlyCollection<DisplayWordCellDto>? Cells);

public sealed record DisplayWordCellDto(char? Value, bool Revealed);