using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using WordoGuessr.Common.Domain;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.Domain.SingleGameEvents;

namespace WordoGuessr.Game.Domain;

public abstract class SingleGame : DomainEntity<Guid>
{
    public const int HalfwayWordHintsTotal = 3;
    public const int RevealLengthHintPenalty = 20;
    public const int RevevalLetterHintTotalPenalty = 80;
    private const int MinDistanceForNeighbourHint = 1;

    public VersionedGameSource VersionedGameSource { get; private set; } = null!;
    public Guid PlayerId { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public int? ClosestGuessDistance { get; private set; }
    public SingleGameStateCode StateCode { get; private set; }

    private readonly List<Guess> _guesses = [];
    public IReadOnlyList<Guess> Guesses => _guesses.AsReadOnly();

    public Guess? LastGuess => _guesses.LastOrDefault();

    private readonly DisplayWord _displayWord = null!;
    public DisplayWordView GetCurrentDisplayWord() =>
        _displayWord.ToView();

    public DisplayWordView GetRevealedDisplayWord() =>
        _displayWord.ToRevealedView();

    private readonly List<Hint> _usedHints = [];
    public IList<Hint> UsedHints => _usedHints.AsReadOnly();

    public readonly IReadOnlyList<int> RevevalHalfwayWordHintPenalties = [10, 20, 50];

    public bool RevealLengthHintUsed => _usedHints.Any(h => h.Type == HintType.RevealLength);
    private int _revealLetterHintsUsedCount => _usedHints.Count(h => h.Type == HintType.RevealLetter);
    private int _halfwayWordHintsUsedCount => _usedHints.Count(h => h.Type == HintType.RevealHalfwayWord);
    private readonly int _revealLetterHintsTotal;
    private readonly int[] _calculatedRevealLetterHintPenalties = [];
    private readonly int[] _orderedLettersIndexesForReveal = [];

    public int HalfwayWordHintsLeft => HalfwayWordHintsTotal - _halfwayWordHintsUsedCount;

    public IReadOnlyList<int>? RevealLetterHintPenalties => RevealLengthHintUsed
        ? _calculatedRevealLetterHintPenalties
        : null;

    public int? RevealLetterHintsLeft => RevealLengthHintUsed
        ? _revealLetterHintsTotal - _revealLetterHintsUsedCount
        : null;

    public int? RevealLetterHintsTotal => RevealLengthHintUsed
        ? _revealLetterHintsTotal
        : null;

    protected readonly SingleGameMode _mode;

    protected SingleGame(
        VersionedGameSource versionedGameSource,
        SingleGameMode mode,
        Guid playerId,
        DateTimeOffset createdAt
        ) : base(Guid.CreateVersion7(createdAt))
    {
        VersionedGameSource = versionedGameSource;
        _mode = mode;
        PlayerId = playerId;
        StateCode = SingleGameStateCode.Active;
        _displayWord = new DisplayWord(versionedGameSource.GameSource.Word);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        _revealLetterHintsTotal = CalculateRevealLetterHintsTotal();
        _calculatedRevealLetterHintPenalties = CalculateRevealLetterHintPenalties();
        _orderedLettersIndexesForReveal = GetOrderedLettersIndexesForReveal();

        AddDomainEvent(new GameCreatedEvent(Id, PlayerId, _mode, VersionedGameSource.Difficulty.Code, CreatedAt));
    }

    protected SingleGame() { }

    public Result<GuessStatus, SingleGameErrorCode> MakeGuess(GuessAttempt attempt)
    {
        if (StateCode != SingleGameStateCode.Active)
        {
            return Result<GuessStatus, SingleGameErrorCode>.Failure(SingleGameErrorCode.GameFinished);
        }

        if (TryGetGuess(attempt.Word, out _))
        {
            return Result<GuessStatus, SingleGameErrorCode>.Success(GuessStatus.AlreadyTried);
        }

        var guessed = attempt.Word == VersionedGameSource.GameSource.Word;
        var guess = new Guess(attempt);
        _guesses.Add(guess);

        if (!ClosestGuessDistance.HasValue || attempt.Distance < ClosestGuessDistance)
        {
            ClosestGuessDistance = attempt.Distance;
        }

        StartedAt ??= attempt.CreatedAt;
        UpdatedAt = attempt.CreatedAt;

        if (guessed)
        {
            StateCode = SingleGameStateCode.Guessed;
            FinishedAt = attempt.CreatedAt;
            AddGameFinishedEvent();
        }

        var status = guessed
            ? GuessStatus.Guessed
            : GuessStatus.NotGuessed;

        return Result<GuessStatus, SingleGameErrorCode>.Success(status);
    }

    public IReadOnlyCollection<Guess> GetGuessesOrderedByDistance() =>
         _guesses
            .OrderBy(guess => guess.Distance)
            .ToArray();

    public Result<SingleGameErrorCode> CanRevealHalfwayWord()
    {
        if (ClosestGuessDistance <= MinDistanceForNeighbourHint)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.HalfWayHintTooClose);
        }

        if (_halfwayWordHintsUsedCount >= HalfwayWordHintsTotal)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.HalfWayHintLimit);
        }

        return Result<SingleGameErrorCode>.Success();
    }

    public Result<GuessStatus, SingleGameErrorCode> RevealHalfwayWord(GuessAttempt attempt)
    {
        var canRevealHalfwayWord = CanRevealHalfwayWord();

        if (!canRevealHalfwayWord.IsSuccess)
            return Result<GuessStatus, SingleGameErrorCode>.Failure(canRevealHalfwayWord.Error);

        var result = MakeGuess(attempt);
        if (!result.IsSuccess)
        {
            return Result<GuessStatus, SingleGameErrorCode>.Failure(result.Error);
        }

        StartedAt ??= attempt.CreatedAt;
        UpdatedAt = attempt.CreatedAt;

        _usedHints.Add(Hint.RevealHalfwayWord(attempt.CreatedAt));

        return Result<GuessStatus, SingleGameErrorCode>.Success(result.Value);
    }

    public Result<Word, SingleGameErrorCode> Surrender(DateTimeOffset at)
    {
        if (StateCode != SingleGameStateCode.Active)
        {
            return Result<Word, SingleGameErrorCode>.Failure(SingleGameErrorCode.GameFinished);
        }

        StateCode = SingleGameStateCode.Surrendered;

        StartedAt ??= at;
        UpdatedAt = at;
        FinishedAt = at;

        AddGameFinishedEvent();

        return Result<Word, SingleGameErrorCode>.Success(VersionedGameSource.GameSource.Word);
    }

    public Result<SingleGameErrorCode> Cancel(DateTimeOffset at)
    {
        if (StateCode != SingleGameStateCode.Active)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.GameFinished);
        }

        StateCode = SingleGameStateCode.Cancelled;
        StartedAt ??= at;
        UpdatedAt = at;
        FinishedAt = at;

        AddGameFinishedEvent();

        return Result<SingleGameErrorCode>.Success();
    }

    public Result<SingleGameErrorCode> RevealWordLength(DateTimeOffset at)
    {
        if (StateCode != SingleGameStateCode.Active)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.GameFinished);
        }

        if (RevealLengthHintUsed)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.LengthAlreadyRevealed);
        }

        _displayWord.InitCells();

        _usedHints.Add(Hint.RevealLength(at));

        StartedAt ??= at;
        UpdatedAt = at;
        return Result<SingleGameErrorCode>.Success();
    }

    public Result<SingleGameErrorCode> RevealRandomLetter(DateTimeOffset at)
    {
        if (StateCode != SingleGameStateCode.Active)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.GameFinished);
        }

        if (!RevealLengthHintUsed)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.LengthShouldBeRevealedFirst);
        }

        if (_revealLetterHintsUsedCount >= _revealLetterHintsTotal)
        {
            return Result<SingleGameErrorCode>.Failure(SingleGameErrorCode.RevealLetterLimit);
        }

        _displayWord.RevealAt(GetCurrentRevealLetterIndex());

        _usedHints.Add(Hint.RevealLetter(at));
        StartedAt ??= at;
        UpdatedAt = at;
        return Result<SingleGameErrorCode>.Success();
    }

    public GameScore GetScore()
    {
        return new GameScore(
            _guesses.Count(g => g.Source == GuessSource.Player),
            BuildPenalties()
        );
    }

    public void UpdateWordsVersion(
        Dictionary<Word, ushort> newGuessesDistances,
        VersionedGameSource versionedGameSource,
        DateTimeOffset at)
    {
        var toDelete = new List<Guess>();
        foreach (var guess in _guesses)
        {
            if (newGuessesDistances.TryGetValue(guess.Word, out var newDistance))
            {
                if (guess.Distance != newDistance)
                {
                    guess.UpdateDistance(newDistance, at);
                }
            }
            else
            {
                toDelete.Add(guess);
            }
        }

        foreach (var guess in toDelete)
        {
            _guesses.Remove(guess);
        }

        ClosestGuessDistance = _guesses.MinBy(g => g.Distance)?.Distance;
        VersionedGameSource = versionedGameSource;
        UpdatedAt = at;
    }

    public abstract bool ShouldShowRevealedWord(DateOnly today, out DisplayWordHideReason? hideReason);

    protected abstract void AddGameFinishedEvent();

    private HintPenalty[] BuildPenalties()
    {
        var result = Enumerable.Empty<HintPenalty>()
            .Concat(BuildHalwayWordPenalties())
            .Concat(BuildRevealLengthPenalty())
            .Concat(BuildRevealLetterPenalties())
            .OrderBy(h => h.Hint.UsedAt)
            .ToArray();

        return result;
    }

    private IEnumerable<HintPenalty> BuildHalwayWordPenalties()
    {
        var result = _usedHints
            .Where(h => h.Type == HintType.RevealHalfwayWord)
            .OrderBy(h => h.UsedAt)
            .Select((hint, index) => new HintPenalty(hint, RevevalHalfwayWordHintPenalties[index]));

        return result;
    }

    private IEnumerable<HintPenalty> BuildRevealLengthPenalty()
    {
        var result = _usedHints
            .Where(h => h.Type == HintType.RevealLength)
            .OrderBy(h => h.UsedAt)
            .Select(hint => new HintPenalty(hint, RevealLengthHintPenalty));

        return result;
    }

    private IEnumerable<HintPenalty> BuildRevealLetterPenalties()
    {
        var result = _usedHints
            .Where(h => h.Type == HintType.RevealLetter)
            .OrderBy(h => h.UsedAt)
            .Select((hint, index) => new HintPenalty(hint, _calculatedRevealLetterHintPenalties[index]));

        return result;
    }

    private int[] CalculateRevealLetterHintPenalties()
    {
        static int GetWeight(int number) => number * number;

        var totalRevealLetterHints = _revealLetterHintsTotal;

        if (totalRevealLetterHints == 0) return [];

        var weights = Enumerable
            .Range(1, totalRevealLetterHints)
            .Select(GetWeight)
            .ToArray();

        var sumOfWeights = weights.Sum();

        var rawPenalties = weights
            .Select(weight => RevevalLetterHintTotalPenalty * ((double)weight / sumOfWeights))
            .ToArray();

        var penalties = rawPenalties
            .Select(x => (int)Math.Floor(x))
            .ToArray();

        var remainder = RevevalLetterHintTotalPenalty - penalties.Sum();

        var fractions = rawPenalties
            .Select((value, index) => new
            {
                Index = index,
                Fraction = value - Math.Floor(value)
            })
            .OrderByDescending(x => x.Fraction)
            .ToArray();

        for (var i = 0; i < remainder; i++)
        {
            penalties[fractions[i].Index]++;
        }

        return penalties;
    }

    private int[] GetOrderedLettersIndexesForReveal()
    {
        static uint CalculateLetterHash(string word, int index)
        {
            var key = $"{index}:{word}";
            var bytes = Encoding.UTF8.GetBytes(key);
            var hash = SHA256.HashData(bytes);
            return BinaryPrimitives.ReadUInt32LittleEndian(hash);
        }

        var word = VersionedGameSource.GameSource.Word.Text;
        var orderedLetters = word
            .Select((_, index) => index)
            .Where(index => index != 0) // не открываем первую букву, т.к. часто это слишком сильная подсказка
            .OrderBy(index => CalculateLetterHash(word, index))
            .ThenBy(index => index)
            .Take(_revealLetterHintsTotal)
            .ToArray();

        if (orderedLetters.Length < _revealLetterHintsTotal)
        {
            throw new InvalidOperationException("Word does not contain enough letters for reveal hints.");
        }

        return orderedLetters;
    }

    private int GetCurrentRevealLetterIndex() => _orderedLettersIndexesForReveal[_revealLetterHintsUsedCount];

    private int CalculateRevealLetterHintsTotal()
    {
        return Math.Max(
            1,
            (int)Math.Round(VersionedGameSource.GameSource.Word.Text.Length / 3d, MidpointRounding.AwayFromZero));
    }

    private bool TryGetGuess(Word word, [NotNullWhen(true)] out Guess? guess)
    {
        guess = null;
        foreach (var existingGuess in _guesses)
        {
            if (existingGuess.Word == word)
            {
                guess = existingGuess;
                return true;
            }
        }

        return false;
    }
}
