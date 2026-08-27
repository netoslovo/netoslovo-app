namespace WordoGuessr.Game.App.Services;

internal static class GuessFillPercentageCalculator
{
    private const double Exponent = 0.33;

    public static double Calculate(int distance, ushort totalWords)
    {
        ArgumentOutOfRangeException.ThrowIfZero(totalWords);

        var normalizedDistance = Math.Clamp((double)distance / totalWords, 0, 1);
        return Math.Clamp(100 * (1 - Math.Pow(normalizedDistance, Exponent)), 0, 100);
    }
}
