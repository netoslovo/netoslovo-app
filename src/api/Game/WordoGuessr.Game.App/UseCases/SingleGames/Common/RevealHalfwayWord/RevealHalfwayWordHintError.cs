namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;

public enum RevealHalfwayWordHintError
{
    GameNotFound,
    GameStateChanged,
    GameSourceNotFound,
    WordsVersionChanged,

    TooCloseToTarget,
    UsageLimit,
    GameFinished
}
