using System.Globalization;
using CsvHelper;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class S3BlockList : IUserNameBlockList
{
    private readonly IObjectStorage _objectStorage;

    public S3BlockList(IObjectStorage objectStorage)
    {
        _objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
    }

    public async Task<IReadOnlyList<string>> GetBlockedWords(int version, CancellationToken ct = default)
    {
        const string blockListFileKey = "blocked_user_name_words.csv";
        var path = ObjectStorageHelpers.GetFullPathForKey(blockListFileKey, version);
        await using var blockListDownload = await _objectStorage.GetAsync(path, ct);
        using var streamReader = new StreamReader(blockListDownload.Content);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var blockedWords = await csvReader
            .GetRecordsAsync<WordRow>(ct)
            .Select(r => r.Word)
            .ToArrayAsync(ct);

        return blockedWords;
    }

    private sealed record WordRow(string Word);
}