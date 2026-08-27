using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class DifficultyMapping
{
    public static DifficultyDto MapToDto(this Difficulty difficulty) => new DifficultyDto(difficulty.Code, difficulty.Name);

    public static IReadOnlyList<DifficultyDto> MapToDto(this IReadOnlyList<Difficulty> difficulties) =>
        difficulties.Select(difficulty => difficulty.MapToDto()).ToList();

    public static Difficulty MapToDomian(this DifficultyDto difficultyDto) => Difficulty.FromCode(difficultyDto.Code);

    public static IReadOnlyList<Difficulty> MapToDomian(this IReadOnlyList<DifficultyDto> difficultiesDto) =>
        difficultiesDto.Select(difficulty => difficulty.MapToDomian()).ToList();
}
