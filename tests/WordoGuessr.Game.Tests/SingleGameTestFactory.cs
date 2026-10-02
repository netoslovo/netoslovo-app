using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Infra;

namespace WordoGuessr.Game.Tests;

internal sealed class SingleGameTestFactory
{
    public const string SecretWord = "слово";

    private static long _lastGameSourceId = 10_000;
    private readonly GameDbContext _dbContext;

    public SingleGameTestFactory(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public VersionedGameSource CreateSource(string word = SecretWord)
    {
        var source = new GameSource(
            Interlocked.Increment(ref _lastGameSourceId),
            Word.Create(word));

        var versionedSource = new VersionedGameSource(
            source.Id,
            1,
            Difficulty.Easy,
            1);

        _dbContext.GameSources.Add(source);
        _dbContext.VersionedGameSources.Add(versionedSource);
        _dbContext.Entry(versionedSource)
            .Reference(candidate => candidate.GameSource)
            .CurrentValue = source;

        return versionedSource;
    }

    public SingleGame CreateArcade(
        DateTimeOffset now,
        VersionedGameSource? source = null) =>
        SingleGame.CreateArcade(
            source ?? CreateSource(),
            Guid.NewGuid(),
            now.AddHours(-1));

    public SingleGame CreateDaily(
        DateOnly gameDay,
        DateTimeOffset now,
        VersionedGameSource? source = null) =>
        SingleGame.CreateDaily(
            source ?? CreateSource(),
            Guid.NewGuid(),
            gameDay,
            now.AddHours(-1));

    public static void Guess(SingleGame game, DateTimeOffset at)
    {
        var result = game.MakeGuess(GuessAttempt.FromPlayer(
            game.VersionedGameSource.GameSource.Word,
            0,
            at));

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Could not prepare guessed game: {result.Error}");
        }
    }

    public static void Surrender(SingleGame game, DateTimeOffset at)
    {
        var result = game.Surrender(at);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Could not prepare surrendered game: {result.Error}");
        }
    }

    public static void Cancel(SingleGame game, DateTimeOffset at)
    {
        var result = game.Cancel(at);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Could not prepare cancelled game: {result.Error}");
        }
    }

    public static void RevealLengthAndOneLetter(SingleGame game, DateTimeOffset at)
    {
        var lengthResult = game.RevealWordLength(at);
        if (!lengthResult.IsSuccess)
        {
            throw new InvalidOperationException($"Could not reveal word length: {lengthResult.Error}");
        }

        var letterResult = game.RevealRandomLetter(at.AddSeconds(1));
        if (!letterResult.IsSuccess)
        {
            throw new InvalidOperationException($"Could not reveal a letter: {letterResult.Error}");
        }
    }
}
