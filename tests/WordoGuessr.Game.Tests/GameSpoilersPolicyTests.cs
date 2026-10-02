using Shouldly;
using WordoGuessr.Game.Domain;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class GameSpoilersPolicyTests : IClassFixture<GameDbContextFixture>
{
    private static readonly DateTimeOffset _now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly _today = DateOnly.FromDateTime(_now.UtcDateTime);
    private readonly GameDbContextFixture _fixture;

    public GameSpoilersPolicyTests(GameDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CanViewDailySpoilers_WithoutViewerGame_ShouldHideSpoilers()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var targetGame = new SingleGameTestFactory(dbContext).CreateDaily(_today, _now);

        // ACT
        var canView = GameSpoilersPolicy.CanViewDailySpoilers(
            targetGame,
            viewerGame: null,
            _today,
            out var hideReason);

        // ASSERT
        canView.ShouldBeFalse();
        hideReason.ShouldBe(GameSpoilersHideReason.ViewerGameNotFinished);
    }

    [Fact]
    public void CanViewDailySpoilers_WithActiveViewerGame_ShouldHideSpoilers()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var factory = new SingleGameTestFactory(dbContext);
        var source = factory.CreateSource();
        var targetGame = factory.CreateDaily(_today, _now, source);
        var viewerGame = factory.CreateDaily(_today, _now, source);

        // ACT
        var canView = GameSpoilersPolicy.CanViewDailySpoilers(
            targetGame,
            viewerGame,
            _today,
            out var hideReason);

        // ASSERT
        canView.ShouldBeFalse();
        hideReason.ShouldBe(GameSpoilersHideReason.ViewerGameNotFinished);
    }

    [Fact]
    public void CanViewDailySpoilers_WithFinishedGameForAnotherSource_ShouldHideSpoilers()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var factory = new SingleGameTestFactory(dbContext);
        var targetGame = factory.CreateDaily(_today, _now);
        var viewerGame = factory.CreateDaily(_today, _now);
        SingleGameTestFactory.Guess(viewerGame, _now);

        // ACT
        var canView = GameSpoilersPolicy.CanViewDailySpoilers(
            targetGame,
            viewerGame,
            _today,
            out var hideReason);

        // ASSERT
        canView.ShouldBeFalse();
        hideReason.ShouldBe(GameSpoilersHideReason.ViewerGameNotFinished);
    }

    [Fact]
    public void CanViewDailySpoilers_AfterSurrenderingCurrentDaily_ShouldHideSpoilersUntilNextDay()
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var factory = new SingleGameTestFactory(dbContext);
        var source = factory.CreateSource();
        var targetGame = factory.CreateDaily(_today, _now, source);
        var viewerGame = factory.CreateDaily(_today, _now, source);
        SingleGameTestFactory.Surrender(viewerGame, _now);

        // ACT
        var canView = GameSpoilersPolicy.CanViewDailySpoilers(
            targetGame,
            viewerGame,
            _today,
            out var hideReason);

        // ASSERT
        canView.ShouldBeFalse();
        hideReason.ShouldBe(GameSpoilersHideReason.ViewerSurrenderedHiddenForToday);
    }

    [Theory]
    [InlineData(SingleGameStateCode.Guessed, 0)]
    [InlineData(SingleGameStateCode.Cancelled, 0)]
    [InlineData(SingleGameStateCode.Surrendered, -1)]
    public void CanViewDailySpoilers_WithCompletedRelevantGame_ShouldShowSpoilers(
        SingleGameStateCode viewerState,
        int gameDayOffset)
    {
        // ARRANGE
        using var dbContext = _fixture.CreateDbContext();
        var factory = new SingleGameTestFactory(dbContext);
        var source = factory.CreateSource();
        var gameDay = _today.AddDays(gameDayOffset);
        var targetGame = factory.CreateDaily(gameDay, _now, source);
        var viewerGame = factory.CreateDaily(gameDay, _now, source);
        Complete(viewerGame, viewerState);

        // ACT
        var canView = GameSpoilersPolicy.CanViewDailySpoilers(
            targetGame,
            viewerGame,
            _today,
            out var hideReason);

        // ASSERT
        canView.ShouldBeTrue();
        hideReason.ShouldBeNull();
    }

    private static void Complete(SingleGame game, SingleGameStateCode state)
    {
        switch (state)
        {
            case SingleGameStateCode.Guessed:
                SingleGameTestFactory.Guess(game, _now);
                break;
            case SingleGameStateCode.Surrendered:
                SingleGameTestFactory.Surrender(game, _now);
                break;
            case SingleGameStateCode.Cancelled:
                SingleGameTestFactory.Cancel(game, _now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Expected a completed game state");
        }
    }
}
