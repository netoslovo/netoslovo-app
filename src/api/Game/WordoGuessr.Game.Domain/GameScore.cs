namespace WordoGuessr.Game.Domain;

public sealed class GameScore
{
    public int GuessesCount { get; }
    public IReadOnlyList<HintPenalty> HintPenalties { get; }

    public GameScore(int guessesCount, IReadOnlyList<HintPenalty> hintPenalties)
    {
        GuessesCount = guessesCount;
        HintPenalties = hintPenalties;
    }

    public int Value => GuessesCount + HintPenalties.Sum(x => x.Penalty);
}