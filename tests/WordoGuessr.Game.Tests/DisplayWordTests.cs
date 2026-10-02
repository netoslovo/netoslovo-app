using Shouldly;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class DisplayWordTests : IClassFixture<GameDbContextFixture>
{
    private static readonly DateTimeOffset _now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly GameDbContextFixture _fixture;

    public DisplayWordTests(GameDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetDisplayWord_BeforeLengthHint_ShouldKeepLengthUnknown()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);

        // ACT
        var displayWord = game.GetDisplayWord();

        // ASSERT
        displayWord.Cells.ShouldBeNull();
    }

    [Fact]
    public void GetDisplayWord_AfterLengthHint_ShouldShowOnlyHiddenPositions()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);
        game.RevealWordLength(_now).IsSuccess.ShouldBeTrue();

        // ACT
        var cells = game.GetDisplayWord().Cells.ShouldNotBeNull();

        // ASSERT
        cells.Count.ShouldBe(SingleGameTestFactory.SecretWord.Length);
        cells.ShouldAllBe(cell => !cell.Revealed && cell.Value == null);
    }

    [Fact]
    public void GetDisplayWord_AfterLetterHint_ShouldRevealExactlyOnePosition()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var game = new SingleGameTestFactory(dbContext).CreateArcade(_now);
        SingleGameTestFactory.RevealLengthAndOneLetter(game, _now);

        // ACT
        var cells = game.GetDisplayWord().Cells.ShouldNotBeNull();

        // ASSERT
        cells.Count(cell => cell.Revealed).ShouldBe(1);
        cells.Count(cell => cell.Value is not null).ShouldBe(1);
        cells.ShouldAllBe(cell => cell.Revealed == (cell.Value != null));
    }
}
