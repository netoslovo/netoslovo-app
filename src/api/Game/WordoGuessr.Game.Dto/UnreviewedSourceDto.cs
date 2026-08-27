namespace WordoGuessr.Game.Dto;

public sealed record UnreviewedSourceDto(GameSourceDto GameSource, IReadOnlyCollection<string> ClosestWords);