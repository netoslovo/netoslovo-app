using Wolverine.Attributes;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Contract.Commands;
using WordoGuessr.Game.Contract.Events;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.IntegrationEventsHandlers;

public static class UploadNewGameSourcesVersionHandler
{
    [Transactional]
    public static async Task<NewGameSourcesVersionUploaded> Handle(
        UploadNewGameSourcesVersion command,
        IGameStore gameStore,
        TimeProvider timeProvider)
    {
        var existingGameSourceIds = gameStore.GameSources
            .Select(gs => gs.Id)
            .ToHashSet();

        var newGameSources = command.GameSources
            .Where(gs => !existingGameSourceIds.Contains(gs.GameSourceId))
            .Select(gs => new GameSource(gs.GameSourceId, Word.Create(gs.Word)));

        gameStore.GameSources.AddRange(newGameSources);

        var existingVersionedGamesSourceIds = gameStore.VersionedGameSources
            .Where(gs => gs.WordsVersion == command.WordsVersion)
            .Select(gs => gs.GameSourceId)
            .ToHashSet();

        var newVersionedGameSources = command.GameSources
            .Where(gs => !existingVersionedGamesSourceIds.Contains(gs.GameSourceId))
            .Select(gs => new VersionedGameSource(
                gs.GameSourceId,
                command.WordsVersion,
                Difficulty.FromCode(gs.DifficultyCode),
                gs.Difficulty
            ));

        gameStore.VersionedGameSources.AddRange(newVersionedGameSources);

        return new NewGameSourcesVersionUploaded(command.SagaId, command.WordsVersion, timeProvider.GetUtcNow());
    }
}
