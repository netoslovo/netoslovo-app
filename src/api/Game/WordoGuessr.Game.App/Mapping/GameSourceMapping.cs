using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GameSourceMapping
{
    public static GameSourceDto MapToDto(this VersionedGameSource versionedGameSource) =>
        new GameSourceDto(
            versionedGameSource.GameSourceId,
            versionedGameSource.GameSource.Word.Text,
            versionedGameSource.Difficulty.MapToDto());
}