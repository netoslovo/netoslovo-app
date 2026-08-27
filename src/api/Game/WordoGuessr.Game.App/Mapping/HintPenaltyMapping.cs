using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class HintPenaltyMapping
{
    public static HintTypeDto MapToDto(this HintType hintType)
    {
        return hintType switch
        {
            HintType.RevealHalfwayWord => HintTypeDto.RevealHalfwayWord,
            HintType.RevealLength => HintTypeDto.RevealLength,
            HintType.RevealLetter => HintTypeDto.RevealLetter,
            _ => throw new ArgumentOutOfRangeException(nameof(hintType), hintType, "Unknown hint type")
        };
    }

    public static UsedHintDto MapToDto(this HintPenalty hintPenalty) =>
        new UsedHintDto(
            hintPenalty.Hint.Type.MapToDto(),
            hintPenalty.Penalty,
            hintPenalty.Hint.UsedAt);

    public static IReadOnlyList<UsedHintDto> MapToDto(this IReadOnlyList<HintPenalty> hintPenalties) =>
        hintPenalties.Select(hintPenalty => hintPenalty.MapToDto()).ToArray();

    public static ScoreDetailsDto MapToDto(this GameScore score) =>
        new ScoreDetailsDto(score.GuessesCount, score.HintPenalties.MapToDto());
}
