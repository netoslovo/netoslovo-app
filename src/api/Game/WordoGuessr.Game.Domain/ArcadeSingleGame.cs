using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.Domain;

public sealed class ArcadeSingleGame : SingleGame
{
    public ArcadeSingleGame(
        VersionedGameSource versionedGameSource,
        Guid playerId,
        DateTimeOffset createdAt) : base(
            versionedGameSource,
            SingleGameMode.Arcade,
            playerId,
            createdAt)
    { }

    private ArcadeSingleGame() { }

    public override bool ShouldShowRevealedWord(DateOnly today, out DisplayWordHideReason? hideReason)
    {
        hideReason = null;

        return StateCode == SingleGameStateCode.Guessed ||
            StateCode == SingleGameStateCode.Surrendered;
    }

    protected override void AddGameFinishedEvent()
    {
        if (StartedAt is null)
        {
            throw new InvalidOperationException("Couldn't add finished event for game without start date");
        }

        var score = GetScore();

        AddDomainEvent(new GameFinishedEvent(
            GameId: Id,
            PlayerId: PlayerId,
            GameSourceId: VersionedGameSource.GameSource.Id,
            DifficultyCode: VersionedGameSource.Difficulty.Code,
            Mode: _mode,
            CreatedAt: CreatedAt,
            StartedAt: StartedAt.Value,
            FinishedAt: FinishedAt!.Value,
            State: StateCode,
            Score: score.Value,
            Attempts: score.GuessesCount,
            RevealHalfwayWordHintsUsed: score.HintPenalties
                .Count(hp => hp.Hint.Type == HintType.RevealHalfwayWord),

            RevealLengthHintUsed: score.HintPenalties
                .Any(hp => hp.Hint.Type == HintType.RevealLength),

            RevealLetterHintsUsed: score.HintPenalties
                .Count(hp => hp.Hint.Type == HintType.RevealLetter),

            DayOfDailyGame: null
        ));
    }
}