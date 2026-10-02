using System.Diagnostics.CodeAnalysis;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Game.Domain;

public sealed class GameSpoilersPolicy
{
    public static Result<Word, SecretWordUnavailableReason> GetSecretWordForOwner(
        SingleGame game,
        DateOnly today)
    {
        var secretWord = game.GetSecretWord();

        if (secretWord.IsSuccess &&
            game.StateCode == SingleGameStateCode.Surrendered &&
            game.IsDailyFor(today))
        {
            return Result<Word, SecretWordUnavailableReason>.Failure(
                SecretWordUnavailableReason.SurrenderedHiddenForToday);
        }

        return secretWord;
    }

    public static bool CanViewDailySpoilers(
        SingleGame targetGame,
        SingleGame? viewerGame,
        DateOnly today,
        [NotNullWhen(false)] out GameSpoilersHideReason? hideReason)
    {
        if (viewerGame is null)
        {
            hideReason = GameSpoilersHideReason.ViewerGameNotFinished;
            return false;
        }

        if (viewerGame.Mode != targetGame.Mode ||
            viewerGame.VersionedGameSource.GameSourceId != targetGame.VersionedGameSource.GameSourceId)
        {
            hideReason = GameSpoilersHideReason.ViewerGameNotFinished;
            return false;
        }

        switch (viewerGame.StateCode)
        {
            case SingleGameStateCode.Guessed:
            case SingleGameStateCode.Cancelled:
            case SingleGameStateCode.Surrendered when !viewerGame.IsDailyFor(today):
                hideReason = null;
                return true;

            case SingleGameStateCode.Active:
                hideReason = GameSpoilersHideReason.ViewerGameNotFinished;
                return false;

            case SingleGameStateCode.Surrendered:
                hideReason = GameSpoilersHideReason.ViewerSurrenderedHiddenForToday;
                return false;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(viewerGame),
                    viewerGame.StateCode,
                    "Unknown single game state");
        }
    }
}