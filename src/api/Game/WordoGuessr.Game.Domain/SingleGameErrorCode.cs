namespace WordoGuessr.Game.Domain;

public enum SingleGameErrorCode
{
    GameFinished,

    HalfWayHintTooClose,
    HalfWayHintLimit,

    LengthAlreadyRevealed,

    RevealLetterLimit,
    LengthShouldBeRevealedFirst,
    LetterAlreadyRevealed
}
