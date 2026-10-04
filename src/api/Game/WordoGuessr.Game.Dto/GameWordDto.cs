using System.Text.Json.Serialization;

namespace WordoGuessr.Game.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "status")]
[JsonDerivedType(typeof(DisplayGameWordDto), "display")]
[JsonDerivedType(typeof(SecretGameWordDto), "secret")]
[JsonDerivedType(typeof(DisplayAndSecretGameWordDto), "displayAndSecret")]
[JsonDerivedType(typeof(UnavailableGameWordDto), "unavailable")]
public abstract record GameWordDto;

public sealed record DisplayGameWordDto(
    DisplayWordDto DisplayWord,
    SecretWordUnavailableReasonDto SecretWordUnavailableReason)
    : GameWordDto;

public sealed record SecretGameWordDto(string Word)
    : GameWordDto;

public sealed record DisplayAndSecretGameWordDto(
    DisplayWordDto DisplayWord,
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