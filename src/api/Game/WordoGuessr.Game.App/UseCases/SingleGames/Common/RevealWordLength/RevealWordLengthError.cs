namespace WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;

public enum RevealWordLengthError
{
    GameNotFound,
    GameStateChanged,

    GameFinished,
    LengthAlreadyRevealed,
}
