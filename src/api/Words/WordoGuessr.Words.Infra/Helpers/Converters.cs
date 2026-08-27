using WordoGuessr.Words.Infra.WordsDistanceStorage;

namespace WordoGuessr.Words.Infra.Helpers;

internal static class Converters
{
    public static ushort ToUshort(int value)
    {
        if (value < ushort.MinValue || value > ushort.MaxValue)
        {
            throw new WordDistancesStoreException($"Value {value} is outside the ushort range.");
        }

        return (ushort)value;
    }
}