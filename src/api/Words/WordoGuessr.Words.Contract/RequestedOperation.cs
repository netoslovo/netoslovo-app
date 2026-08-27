using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Words.Contract;

public static class RequestOperation
{
    public sealed record TotalWords();

    public sealed record DistancesToWords(Word SourceWord, IReadOnlyList<Word> TargetWords);

    public sealed record DistanceToWord(Word SourceWord, Word TargetWord);

    public sealed record WordByDistance(Word SourceWord, Func<ushort, ushort> FuncOfTotalWordsCount);

    public sealed record ClosestWords(Word SourceWord, ushort WordsCount);
}
