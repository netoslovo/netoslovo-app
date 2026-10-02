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
                SharedGameSpoilersHideReasonDto.HiddenForToday,

            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown game spoilers hide reason")
        };

    public static SharedGameSpoilersHideReasonDtoV2 MapToDtoV2(this GameSpoilersHideReason reason) =>
        reason switch
        {
            GameSpoilersHideReason.ViewerGameNotFinished =>
                SharedGameSpoilersHideReasonDtoV2.ViewerGameNotFinished,

            GameSpoilersHideReason.ViewerSurrenderedHiddenForToday =>
                SharedGameSpoilersHideReasonDtoV2.ViewerSurrenderedHiddenForToday,

            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Unknown game spoilers hide reason")
        };
}
