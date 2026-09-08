using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

public sealed class PgWordsDistanceStoreLoaderOptions : INamedOptions
{
    public static string Name => "Words:PgWordsDistanceStoreLoader";

    [Range(1, 50000)]
    public int? DistanceLoadingBatchSize { get; set; }
}