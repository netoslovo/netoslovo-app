namespace WordoGuessr.Game.Dto;

public sealed record TextHintDto(
    DisplayWordDtoV2 DisplayWord,
    HintsInfoDto HintsInfo,
    int Score,
    ScoreDetailsDto ScoreDetails);
