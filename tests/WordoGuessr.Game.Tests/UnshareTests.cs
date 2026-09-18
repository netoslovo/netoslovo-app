using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.Unshare;
using WordoGuessr.Game.Domain;
using Xunit;

namespace WordoGuessr.Game.Tests;

public sealed class UnshareTests : IClassFixture<GameDbContextFixture>
{
    private readonly GameDbContextFixture _fixture;

    public UnshareTests(GameDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Unshare_ShouldDeleteOnlyTheOwnersSelectedShare(bool isOwner)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var dbContext = _fixture.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var playerId = Guid.NewGuid();
        var source = new GameSource(Random.Shared.NextInt64(1, long.MaxValue), Word.Create("слово"));
        var versionedSource = new VersionedGameSource(source.Id, 1, Difficulty.Easy, 1);
        dbContext.GameSources.Add(source);
        dbContext.VersionedGameSources.Add(versionedSource);
        dbContext.Entry(versionedSource).Reference(vgs => vgs.GameSource).CurrentValue = source;

        var game = SingleGame.CreateArcade(versionedSource, playerId, now);
        var otherGame = SingleGame.CreateArcade(versionedSource, playerId, now);
        dbContext.SingleGames.AddRange(game, otherGame);
        dbContext.SingleGameShares.AddRange(
            new SingleGameShare(game.Id, now, true),
            new SingleGameShare(otherGame.Id, now, false));
        await dbContext.SaveChangesAsync(ct);
        dbContext.ChangeTracker.Clear();

        await using var services = new ServiceCollection()
            .AddGameApp()
            .AddScoped<IGameStore>(_ => dbContext)
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<UnshareCommand>>();
        var command = new UnshareCommand(isOwner ? playerId : Guid.NewGuid(), game.Id);

        await handler.Handle(command, ct);
        await handler.Handle(command, ct);

        (await dbContext.SingleGameShares.AnyAsync(sgs => sgs.Id == game.Id, ct)).ShouldBe(!isOwner);
        (await dbContext.SingleGameShares.AnyAsync(sgs => sgs.Id == otherGame.Id, ct)).ShouldBeTrue();
        (await dbContext.SingleGames.AnyAsync(sg => sg.Id == game.Id, ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task Unshare_ShouldSucceedWhenGameDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var dbContext = _fixture.CreateDbContext();
        await using var services = new ServiceCollection()
            .AddGameApp()
            .AddScoped<IGameStore>(_ => dbContext)
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<UnshareCommand>>();

        await handler.Handle(new UnshareCommand(Guid.NewGuid(), Guid.NewGuid()), ct);
    }
}
