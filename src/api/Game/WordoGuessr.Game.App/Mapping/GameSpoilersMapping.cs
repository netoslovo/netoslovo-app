using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GameSpoilersMapping
{
    public static SharedGameSpoilersHideReasonDto MapToDto(this GameSpoilersHideReason reason) =>
        reason switch
        {
            GameSpoilersHideReason.ViewerGameNotFinished =>
                SharedGameSpoilersHideReasonDto.ViewerGameNotFinished,

            GameSpoilersHideReason.ViewerSurrenderedHiddenForToday =>
                SharedGameSpoilersHideReasonDto.ViewerSurrenderedHiddenForToday,

            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown game spoilers hide reason")
        };
}
