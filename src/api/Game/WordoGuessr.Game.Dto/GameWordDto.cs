using System.Text.Json.Serialization;

namespace WordoGuessr.Game.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "status")]
[JsonDerivedType(typeof(DisplayGameWordDto), "display")]
[JsonDerivedType(typeof(SecretGameWordDto), "secret")]
[JsonDerivedType(typeof(DisplayAndSecretGameWordDto), "displayAndSecret")]
[JsonDerivedType(typeof(UnavailableGameWordDto), "unavailable")]
public abstract record GameWordDto;

public sealed record DisplayGameWordDto(
    DisplayWordDtoV2 DisplayWord,
    SecretWordUnavailableReasonDto SecretWordUnavailableReason)
    : GameWordDto;

public sealed record SecretGameWordDto(string Word)
    : GameWordDto;

public sealed record DisplayAndSecretGameWordDto(
    DisplayWordDtoV2 DisplayWord,
    string SecretWord)
    : GameWordDto;

public sealed record UnavailableGameWordDto(
    GameWordUnavailableReasonDto Reason)
    : GameWordDto;

public enum GameWordUnavailableReasonDto
{
    GameCancelled
}

public enum SecretWordUnavailableReasonDto
{
    GameInProgress,
    GameCancelled,
    SurrenderedHiddenForToday
}