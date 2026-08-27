namespace WordoGuessr.Game.Dto;

public sealed record GuessHintDto(
    GuessOutcomeDto GuessOutcome,
    HintsInfoDto HintsInfo);
