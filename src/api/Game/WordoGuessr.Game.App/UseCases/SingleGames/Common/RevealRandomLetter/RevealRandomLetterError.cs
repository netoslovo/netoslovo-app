namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;

public enum RevealRandomLetterError
{
    GameNotFound,
    GameStateChanged,

    GameFinished,
    RevealLetterLimit,
    LengthShouldBeRevealedFirst,
}
