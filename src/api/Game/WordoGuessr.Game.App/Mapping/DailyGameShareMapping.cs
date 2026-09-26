using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DailyGameShareMapping
{
    public static DailyGameShareDto MapToDto(this DailyGameShare share) =>
        new DailyGameShareDto(share.PublicId);
}
