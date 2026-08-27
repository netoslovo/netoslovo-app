using System.Globalization;
using CsvHelper;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class S3ReplacementList : IUserNameReplacementList
{
    private readonly IObjectStorage _objectStorage;

    public S3ReplacementList(IObjectStorage objectStorage)
    {
        _objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
    }

    public async Task<IReadOnlyList<UserNameReplacement>> GetReplacements(
        int version, CancellationToken ct = default)
    {
        const string replacementListFileKey = "user_name_replacements.csv";
        var path = ObjectStorageHelpers.GetFullPathForKey(replacementListFileKey, version);
        await using var replacementListDownload = await _objectStorage.GetAsync(path, ct);
        using var streamReader = new StreamReader(replacementListDownload.Content);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        var replacements = await csvReader
            .GetRecordsAsync<UserNameReplacement>(ct)
            .ToArrayAsync(ct);

        return replacements;
    }

}