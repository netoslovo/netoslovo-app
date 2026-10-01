namespace WordoGuessr.Game.Dto;

public sealed record DisplayWordDtoV2(IReadOnlyCollection<DisplayWordCellDtoV2>? Cells);

public sealed record DisplayWordCellDtoV2(char? Value, bool Revealed);