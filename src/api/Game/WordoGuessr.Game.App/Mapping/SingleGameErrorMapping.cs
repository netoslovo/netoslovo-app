using WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.Surrender;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.Mapping;

internal static class SingleGameErrorMapping
{
    public static MakeGuessError MapToMakeGuessError(this SingleGameErrorCode errorCode) =>
        errorCode switch
        {
            SingleGameErrorCode.GameFinished => MakeGuessError.GameFinished,
            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Unknown error code")
        };

    public static SurrenderError MapToSurrenderError(this SingleGameErrorCode errorCode) =>
        errorCode switch
        {
            SingleGameErrorCode.GameFinished => SurrenderError.GameFinished,
            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Unknown error code")
        };

    public static RevealHalfwayWordHintError MapToRevealHalfwayWordHintError(this SingleGameErrorCode errorCode) =>
        errorCode switch
        {
            SingleGameErrorCode.GameFinished => RevealHalfwayWordHintError.GameFinished,
            SingleGameErrorCode.HalfWayHintLimit => RevealHalfwayWordHintError.UsageLimit,
            SingleGameErrorCode.HalfWayHintTooClose => RevealHalfwayWordHintError.TooCloseToTarget,
            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Unknown error code")
        };

    public static RevealWordLengthError MapToRevealWordLengthError(this SingleGameErrorCode errorCode) =>
        errorCode switch
        {
            SingleGameErrorCode.GameFinished => RevealWordLengthError.GameFinished,
            SingleGameErrorCode.LengthAlreadyRevealed => RevealWordLengthError.LengthAlreadyRevealed,
            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Unknown error code")
        };

    public static RevealRandomLetterError MapToRevealRandomLetterError(this SingleGameErrorCode errorCode) =>
        errorCode switch
        {
            SingleGameErrorCode.GameFinished => RevealRandomLetterError.GameFinished,
            SingleGameErrorCode.LengthShouldBeRevealedFirst => RevealRandomLetterError.LengthShouldBeRevealedFirst,
            SingleGameErrorCode.RevealLetterLimit => RevealRandomLetterError.RevealLetterLimit,
            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, "Unknown error code")
        };
}
