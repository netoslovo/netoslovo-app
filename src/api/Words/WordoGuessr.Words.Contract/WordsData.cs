using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Words.Contract;

public sealed class WordsData
{
    private readonly ushort? _totalWords;
    private readonly Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>? _distancesToWords;
    private readonly Result<WordDistance, WordPairDataErrorCode>? _distanceToWord;
    private readonly Result<WordDistance, SourceWordDataErrorCode>? _wordByDistance;
    private readonly Result<IReadOnlyList<Word>, SourceWordDataErrorCode>? _closestWords;

    public int Version { get; }

    public ushort TotalWords =>
        _totalWords ?? throw new NotRequestedException(nameof(TotalWords));

    public Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode> DistancesToWords =>
        _distancesToWords ?? throw new NotRequestedException(nameof(DistancesToWords));

    public Result<WordDistance, WordPairDataErrorCode> DistanceToWord =>
        _distanceToWord ?? throw new NotRequestedException(nameof(DistanceToWord));

    public Result<WordDistance, SourceWordDataErrorCode> WordByDistance =>
        _wordByDistance ?? throw new NotRequestedException(nameof(WordByDistance));

    public Result<IReadOnlyList<Word>, SourceWordDataErrorCode> ClosestWords =>
        _closestWords ?? throw new NotRequestedException(nameof(ClosestWords));

    public WordsData(
        int version,
        ushort? totalWords = null,
        Result<IReadOnlyList<NullableDistance>, SourceWordDataErrorCode>? distancesToWords = null,
        Result<WordDistance, WordPairDataErrorCode>? distanceToWord = null,
        Result<WordDistance, SourceWordDataErrorCode>? wordByDistance = null,
        Result<IReadOnlyList<Word>, SourceWordDataErrorCode>? closestWords = null)
    {
        Version = version;
        _totalWords = totalWords;
        _distancesToWords = distancesToWords;
        _distanceToWord = distanceToWord;
        _wordByDistance = wordByDistance;
        _closestWords = closestWords;
    }
}
