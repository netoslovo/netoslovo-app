namespace WordoGuessr.Game.Dto;

public sealed record class HintsInfoDto(
    bool RevealLengthHintUsed,
    int? RevealLetterHintsTotal,
    int? RevealLetterHintsLeft,
    int NeighbourHintsTotal,
    int NeighbourHintsLeft,
    int RevealLengthHintPenalty,
    IReadOnlyList<int>? RevealLetterHintPenalties,
    IReadOnlyList<int> RevealHalfwayWordHintPenalties);
