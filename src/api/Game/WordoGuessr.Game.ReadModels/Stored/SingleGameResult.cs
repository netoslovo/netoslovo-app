using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.ReadModels.Stored;

public sealed class SingleGameResult
{
    public Guid GameId { get; }
    public long GameSourceId { get; }
    public SingleGameMode Mode { get; }
    public string DifficultyCode { get; } = null!;
    public DateOnly? DayOfDailyGame { get; }
    public Guid PlayerId { get; }
    public TimeSpan Duration { get; }
    public int Score { get; }
    public int Attempts { get; }
    public int RevealHalfwayWordHintsUsed { get; }
    public bool RevealLengthHintUsed { get; }
    public int RevealLetterHintsUsed { get; }
    public SingleGameStateCode State { get; }

    private SingleGameResult(
        Guid gameId,
        long gameSourceId,
        SingleGameMode mode,
        string difficultyCode,
        Guid playerId,
        TimeSpan duration,
        int score,
        int attempts,
        int revealHalfwayWordHintsUsed,
        bool revealLengthHintUsed,
        int revealLetterHintsUsed,
        SingleGameStateCode state,
        DateOnly? dayOfDailyGame = null)
    {
        GameId = gameId;
        GameSourceId = gameSourceId;
        Mode = mode;
        DifficultyCode = difficultyCode;
        DayOfDailyGame = dayOfDailyGame;
        PlayerId = playerId;
        Duration = duration;
        Score = score;
        Attempts = attempts;
        RevealHalfwayWordHintsUsed = revealHalfwayWordHintsUsed;
        RevealLengthHintUsed = revealLengthHintUsed;
        RevealLetterHintsUsed = revealLetterHintsUsed;
        State = state;
    }

    private SingleGameResult() { }

    public static SingleGameResult FromGameFinishedEvent(GameFinishedEvent @event) =>
        new SingleGameResult(
            gameId: @event.GameId,
            gameSourceId: @event.GameSourceId,
            mode: @event.Mode,
            difficultyCode: @event.DifficultyCode,
            playerId: @event.PlayerId,
            duration: @event.FinishedAt - @event.StartedAt,
            score: @event.Score,
            attempts: @event.Attempts,
            revealHalfwayWordHintsUsed: @event.RevealHalfwayWordHintsUsed,
            revealLengthHintUsed: @event.RevealLengthHintUsed,
            revealLetterHintsUsed: @event.RevealLetterHintsUsed,
            state: @event.State,
            dayOfDailyGame: @event.DayOfDailyGame
        );
}
