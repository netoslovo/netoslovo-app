using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.Domain.SingleGameEvents;

public record GameFinishedEvent(
    Guid GameId,
    Guid PlayerId,
    long GameSourceId,
    string DifficultyCode,
    SingleGameMode Mode,
    DateTimeOffset CreatedAt,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    SingleGameStateCode State,
    int Score,
    int Attempts,
    int RevealHalfwayWordHintsUsed,
    bool RevealLengthHintUsed,
    int RevealLetterHintsUsed,
    DateOnly? DayOfDailyGame = null) : IDomainEvent;
