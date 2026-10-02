using Shouldly;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Dto;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class GameWordMappingTests : IClassFixture<GameDbContextFixture>
{
    private static readonly DateTimeOffset _now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly _today = DateOnly.FromDateTime(_now.UtcDateTime);
    private readonly GameDbContextFixture _fixture;

    public GameWordMappingTests(GameDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void BuildGameWordDto_ForActiveGame_ShouldExposeOnlyCurrentDisplayWord()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);

        // ACT
        var gameWord = game.BuildGameWordDto(_today).ShouldBeOfType<DisplayGameWordDto>();

        // ASSERT
        gameWord.SecretWordUnavailableReason.ShouldBe(SecretWordUnavailableReasonDto.GameInProgress);
        gameWord.DisplayWord.Cells.ShouldBeNull();
    }

    [Fact]
    public void BuildGameWordDto_ForGuessedGame_ShouldExposeSecretWord()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);
        SingleGameTestFactory.Guess(game, _now);

        // ACT
        var gameWord = game.BuildGameWordDto(_today).ShouldBeOfType<SecretGameWordDto>();

        // ASSERT
        gameWord.Word.ShouldBe(SingleGameTestFactory.SecretWord);
    }

    [Fact]
    public void BuildGameWordDto_ForSurrenderedCurrentDaily_ShouldExposeOnlyProgress()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateDaily(_today, _now);
        SingleGameTestFactory.RevealLengthAndOneLetter(game, _now);
        SingleGameTestFactory.Surrender(game, _now.AddMinutes(1));

        // ACT
        var gameWord = game.BuildGameWordDto(_today).ShouldBeOfType<DisplayGameWordDto>();

        // ASSERT
        gameWord.SecretWordUnavailableReason.ShouldBe(
            SecretWordUnavailableReasonDto.SurrenderedHiddenForToday);
        AssertOneLetterRevealed(gameWord.DisplayWord);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildGameWordDto_ForSurrenderedNonCurrentDailyOrArcade_ShouldExposeSecretAndProgress(
        bool isPastDaily)
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var factory = new SingleGameTestFactory(dbContext);
        var game = isPastDaily
            ? factory.CreateDaily(_today.AddDays(-1), _now)
            : factory.CreateArcade(_now);
        SingleGameTestFactory.RevealLengthAndOneLetter(game, _now);
        SingleGameTestFactory.Surrender(game, _now.AddMinutes(1));

        // ACT
        var gameWord = game.BuildGameWordDto(_today).ShouldBeOfType<DisplayAndSecretGameWordDto>();

        // ASSERT
        gameWord.SecretWord.ShouldBe(SingleGameTestFactory.SecretWord);
        AssertOneLetterRevealed(gameWord.DisplayWord);
    }

    [Fact]
    public void BuildGameWordDto_ForCancelledGame_ShouldNotExposeAnyWord()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);
        SingleGameTestFactory.RevealLengthAndOneLetter(game, _now);
        SingleGameTestFactory.Cancel(game, _now.AddMinutes(1));

        // ACT
        var gameWord = game.BuildGameWordDto(_today).ShouldBeOfType<UnavailableGameWordDto>();

        // ASSERT
        gameWord.Reason.ShouldBe(GameWordUnavailableReasonDto.GameCancelled);
    }

    [Fact]
    public void BuildSharedGameWordDto_ForVisibleSurrenderedDaily_ShouldExposeSecretAndProgress()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateDaily(_today, _now);
        SingleGameTestFactory.RevealLengthAndOneLetter(game, _now);
        SingleGameTestFactory.Surrender(game, _now.AddMinutes(1));

        // ACT
        var gameWord = game.BuildSharedGameWordDto().ShouldBeOfType<DisplayAndSecretGameWordDto>();

        // ASSERT
        gameWord.SecretWord.ShouldBe(SingleGameTestFactory.SecretWord);
        AssertOneLetterRevealed(gameWord.DisplayWord);
    }

    private static void AssertOneLetterRevealed(DisplayWordDtoV2 displayWord)
    {
        var cells = displayWord.Cells.ShouldNotBeNull();
        cells.Count(cell => cell.Revealed).ShouldBe(1);
        cells.Count(cell => cell.Value is not null).ShouldBe(1);
    }
}
