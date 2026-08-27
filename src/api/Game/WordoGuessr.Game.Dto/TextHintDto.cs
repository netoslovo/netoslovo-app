namespace WordoGuessr.Game.Dto;

public sealed record TextHintDto(
    DisplayWordDto DisplayWord,
    HintsInfoDto HintsInfo,
    int Score,
    ScoreDetailsDto ScoreDetails);
