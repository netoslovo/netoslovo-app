using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.Mapping;

internal static class GameMapping
{
    public static GameDto MapToDto(
        this SingleGame singleGame,
        DateOnly today,
        GuessDto? lastGuess = default,
        IReadOnlyCollection<GuessDto>? guesses = default)
    {
        var score = singleGame.GetScore();

        return new GameDto(
            singleGame.Id,
            singleGame.VersionedGameSource.Difficulty.MapToDto(),
            singleGame.StateCode.MapToDto(),
            lastGuess,
            guesses ?? [],
            singleGame.GetDisplayWordLegacy(today, out var unavailableReason).MapToDto(unavailableReason),
            singleGame.BuildGameWordDto(today),
            singleGame.BuildHintsInfoDto(),
            score.Value,
            score.MapToDto());
    }

    public static GameStateDto MapToDto(this SingleGameStateCode state)
    {
        return state switch
        {
            SingleGameStateCode.Active => GameStateDto.Active,
            SingleGameStateCode.Guessed => GameStateDto.Guessed,
            SingleGameStateCode.Surrendered => GameStateDto.Surrendered,
            SingleGameStateCode.Cancelled => GameStateDto.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown state")
        };
    }

    public static GameWordDto BuildGameWordDto(
        this SingleGame game,
        DateOnly today) =>
            BuildGameWordDto(game, GameSpoilersPolicy.GetSecretWordForOwner(game, today));

    public static GameWordDto BuildSharedGameWordDto(this SingleGame game) =>
        BuildGameWordDto(game, game.GetSecretWord());

    private static GameWordDto BuildGameWordDto(
        SingleGame game,
        Result<Word, SecretWordUnavailableReason> secretWordResult)
    {
        // если SecretWord скрыт из-за GameCancelled, то DisplayWord тоже не показываем
        if (!secretWordResult.IsSuccess)
        {
            return secretWordResult.Error == SecretWordUnavailableReason.GameCancelled
                ? new UnavailableGameWordDto(
                    GameWordUnavailableReasonDto.GameCancelled)
                : new DisplayGameWordDto(
                    game.GetDisplayWord().MapToDtoV2(),
                    secretWordResult.Error.MapToDto());
        }

        // если SecretWord открыт, то для Surrendered отдаём еще и Display,
        // чтобы видеть, где игрок остановился
        return game.StateCode == SingleGameStateCode.Surrendered
            ? new DisplayAndSecretGameWordDto(
                game.GetDisplayWord().MapToDtoV2(),
                secretWordResult.Value.Text)
            : new SecretGameWordDto(
                secretWordResult.Value.Text);
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

    public static HintsInfoDto BuildHintsInfoDto(this SingleGame game)
    {
        return new HintsInfoDto
        (
            RevealLengthHintUsed: game.RevealLengthHintUsed,
            NeighbourHintsTotal: SingleGame.HalfwayWordHintsTotal,
            NeighbourHintsLeft: game.HalfwayWordHintsLeft,
            RevealLetterHintsTotal: game.RevealLetterHintsTotal,
            RevealLetterHintsLeft: game.RevealLetterHintsLeft,
            RevealLengthHintPenalty: SingleGame.RevealLengthHintPenalty,
            RevealLetterHintPenalties: game.RevealLetterHintPenalties,
            RevealHalfwayWordHintPenalties: game.RevevalHalfwayWordHintPenalties
        );
    }
}
