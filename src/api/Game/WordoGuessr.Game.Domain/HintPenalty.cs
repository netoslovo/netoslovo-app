namespace WordoGuessr.Game.Domain;

public sealed class HintPenalty
{
    public Hint Hint { get; }
    public int Penalty { get; }

    public HintPenalty(Hint hint, int penalty)
    {
        Hint = hint;
        Penalty = penalty;
    }
}