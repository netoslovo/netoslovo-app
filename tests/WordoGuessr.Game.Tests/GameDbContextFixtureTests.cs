using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class GameDbContextFixtureTests : IClassFixture<GameDbContextFixture>
{
    private readonly GameDbContextFixture _fixture;

    public GameDbContextFixtureTests(GameDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateDbContext_ShouldCreateConnectedContext()
    {
        var ct = TestContext.Current.CancellationToken;

        // ACT
        await using var dbContext = _fixture.CreateDbContext();
        var canConnect = await dbContext.Database.CanConnectAsync(ct);

        // ASSERT
        canConnect.ShouldBeTrue();
    }
}
