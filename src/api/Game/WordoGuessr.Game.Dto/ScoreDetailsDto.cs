namespace WordoGuessr.Game.Dto;

public sealed record ScoreDetailsDto
(
    int GuessesCount,
    IReadOnlyList<UsedHintDto> UsedHints
);