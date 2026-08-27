using WordoGuessr.Common.Domain;

namespace WordoGuessr.Game.Domain.SingleGameEvents;

public sealed record GameCreatedEvent(
    Guid GameId,
    Guid PlayerId,
    SingleGameMode Mode,
    string DifficultyCode,
    DateTimeOffset CreatedAt) : IDomainEvent;
