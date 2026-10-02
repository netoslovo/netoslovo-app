using System.Text.Json.Serialization;

namespace WordoGuessr.Game.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "visibility")]
[JsonDerivedType(typeof(VisibleSharedDailyGameSpoilersDto), "visible")]
[JsonDerivedType(typeof(HiddenSharedDailyGameSpoilersDto), "hidden")]
public abstract record SharedDailyGameSpoilersDto;

public sealed record VisibleSharedDailyGameSpoilersDto(
    GameWordDto GameWord,
    IReadOnlyCollection<VisibleSharedGuessDto> Guesses)
    : SharedDailyGameSpoilersDto;

public sealed record HiddenSharedDailyGameSpoilersDto(
    SharedGameSpoilersHideReasonDtoV2 Reason,
    IReadOnlyCollection<HiddenSharedGuessDto> Guesses)
    : SharedDailyGameSpoilersDto;