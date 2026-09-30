using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class SecretWordMapping
{
    public static SecretWordDto MapToDto(
        this Result<Word, SecretWordUnavailableReason> wordResult)
    {
        return wordResult.IsSuccess
            ? new AvailableSecretWordDto(wordResult.Value.Text)
            : new UnavailableSecretWordDto(wordResult.Error.MapToDto());
    }

    public static SecretWordUnavailableReasonDto MapToDto(
        this SecretWordUnavailableReason reason) =>
        reason switch
        {
            SecretWordUnavailableReason.GameInProgress => SecretWordUnavailableReasonDto.GameInProgress,
            SecretWordUnavailableReason.GameCancelled => SecretWordUnavailableReasonDto.GameCancelled,
            SecretWordUnavailableReason.SurrenderedHiddenForToday => SecretWordUnavailableReasonDto.SurrenderedHiddenForToday,
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown reason")
        };
}
