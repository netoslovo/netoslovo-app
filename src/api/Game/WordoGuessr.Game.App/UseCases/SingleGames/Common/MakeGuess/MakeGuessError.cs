namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;

public enum MakeGuessError
{
    WordNotFound,
    InvalidInput,
    GameNotFound,
    GameStateChanged,
    GameSourceNotFound,
    WordsVersionChanged,
    GameFinished
}
