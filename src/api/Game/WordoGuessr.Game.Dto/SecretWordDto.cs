using System.Text.Json.Serialization;

namespace WordoGuessr.Game.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "status")]
[JsonDerivedType(typeof(AvailableSecretWordDto), "available")]
[JsonDerivedType(typeof(UnavailableSecretWordDto), "unavailable")]
public abstract record SecretWordDto
{
    private protected SecretWordDto() { }
}

public sealed record AvailableSecretWordDto(string Word)
    : SecretWordDto;

public sealed record UnavailableSecretWordDto(SecretWordUnavailableReasonDto Reason)
    : SecretWordDto;

public enum SecretWordUnavailableReasonDto
{
    GameInProgress,
    GameCancelled,
    SurrenderedHiddenForToday
}