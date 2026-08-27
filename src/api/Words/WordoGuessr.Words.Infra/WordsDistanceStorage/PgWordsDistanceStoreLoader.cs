using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Infra.Database;

namespace WordoGuessr.Words.Infra.WordsDistanceStorage;

public sealed class PgWordsDistanceStoreLoader : IWordsDistanceStoreLoader
{
    private const string SchemaName = "words";
    private const string WordsVersionTableName = "words_versions";
    private const string WordsIndexTableName = "words_indexes";
    private const string WordsDistanceMapsTableName = "word_distance_maps";
    private readonly WordsDbContext _wordsDbContext;
    private readonly NpgsqlConnection _connection;

    private readonly TimeProvider _timeProvider;

    public PgWordsDistanceStoreLoader(WordsDbContext wordsDbContext, TimeProvider timeProvider)
    {
        _wordsDbContext = wordsDbContext ?? throw new ArgumentNullException(nameof(wordsDbContext));

        if (wordsDbContext.Database.GetDbConnection() is not NpgsqlConnection npgsqlConnection)
        {
            throw new InvalidOperationException(
                $"{nameof(PgWordsDistanceStoreLoader)} requires an Npgsql connection.");
        }

        _connection = npgsqlConnection;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task InsertWordsVersion(int version, CancellationToken ct = default)
    {
        await Execute(
        $"""
        INSERT INTO {SchemaName}.{WordsVersionTableName}
            (version, state, created_at)
        VALUES (@version, @state, @created_at)
        ON CONFLICT DO NOTHING
        """,
        ct,
        parameters: [
            new NpgsqlParameter<int>("version", version),
            new NpgsqlParameter<int>("state", (int)WordsVersionState.Created),
            new NpgsqlParameter<DateTimeOffset>("created_at", _timeProvider.GetUtcNow())]);
    }

    public async Task ActivateWordsVersion(int version, CancellationToken ct = default)
    {
        await using var transaction = await _wordsDbContext.Database.BeginTransactionAsync(ct);

        await _wordsDbContext.WordsVersions
            .Where(wv => wv.Version != version && wv.State == WordsVersionState.Active)
            .ExecuteUpdateAsync(
                setters =>
                {
                    setters.SetProperty(wv => wv.State, WordsVersionState.Retired);
                    setters.SetProperty(wv => wv.RetiredAt, _timeProvider.GetUtcNow());
                },
                ct);

        await _wordsDbContext.WordsVersions
            .Where(wv => wv.Version == version && wv.State == WordsVersionState.Created)
            .ExecuteUpdateAsync(
                setters =>
                {
                    setters.SetProperty(wv => wv.State, WordsVersionState.Active);
                    setters.SetProperty(wv => wv.ActivatedAt, _timeProvider.GetUtcNow());
                },
                ct);

        await transaction.CommitAsync(ct);
    }

    public async Task DeleteOldWordsVersions(int upToVersion, CancellationToken ct = default)
    {
        for (int version = 1; version < upToVersion; version++)
        {
            var wordIndexPartitionName = GetPartitionName(WordsIndexTableName, version);
            var wordDistanceMapPartitionName = GetPartitionName(WordsDistanceMapsTableName, version);

            await DetachPartitionIfAttached(WordsIndexTableName, wordIndexPartitionName, ct);
            await DetachPartitionIfAttached(WordsDistanceMapsTableName, wordDistanceMapPartitionName, ct);

            await DropTableIfEsixts(wordIndexPartitionName, ct);
            await DropTableIfEsixts(wordDistanceMapPartitionName, ct);
        }
    }

    public async Task<int> InsertWordsIndexes(
        IAsyncEnumerable<WordIndex> wordsIndexes,
        int version,
        CancellationToken ct = default)
    {
        var partitionName = GetPartitionName(WordsIndexTableName, version);
        var attached = await IsPartitionAttached(WordsIndexTableName, partitionName, ct);

        if (attached)
        {
            var indexCount = await _wordsDbContext.WordsIndexes
                .CountAsync(wi => wi.Version == version, ct);

            return indexCount;
        }

        await CreateTableFromParent(WordsIndexTableName, partitionName, version, ct);

        int counter = 0;
        await using (var importer = await _connection.BeginBinaryImportAsync(
            $"""
            COPY {SchemaName}.{partitionName}
                (version, word_id, word_text)
            FROM STDIN (FORMAT BINARY)
            """,
            ct))
        {
            await foreach (var row in wordsIndexes.WithCancellation(ct))
            {
                await importer.StartRowAsync(ct);
                await importer.WriteAsync(row.Version, NpgsqlDbType.Integer, ct);
                await importer.WriteAsync(row.WordId, NpgsqlDbType.Integer, ct);
                await importer.WriteAsync(row.WordText, NpgsqlDbType.Text, ct);
                counter++;
            }

            await importer.CompleteAsync(ct);
        }

        await AttachPartition(WordsIndexTableName, partitionName, version, ct);
        return counter;
    }

    public async Task InsertWordDistanceMaps(
        IAsyncEnumerable<WordDistanceMap> wordDistanceMaps,
        int version,
        CancellationToken ct = default)
    {
        var partitionName = GetPartitionName(WordsDistanceMapsTableName, version);
        var attached = await IsPartitionAttached(WordsDistanceMapsTableName, partitionName, ct);
        if (attached) return;

        await CreateTableFromParent(WordsDistanceMapsTableName, partitionName, version, ct);

        await using (var importer = await _connection.BeginBinaryImportAsync(
            $"""
            COPY {SchemaName}.{partitionName}
                (version, word_id, words_ids_by_distance, distances_by_words_ids)
            FROM STDIN (FORMAT BINARY)
            """,
            ct))
        {
            await foreach (var row in wordDistanceMaps.WithCancellation(ct))
            {
                await importer.StartRowAsync(ct);
                await importer.WriteAsync(row.Version, NpgsqlDbType.Integer, ct);
                await importer.WriteAsync(row.WordId, NpgsqlDbType.Integer, ct);
                await importer.WriteAsync(row.WordsIdsByDistance, NpgsqlDbType.Bytea, ct);
                await importer.WriteAsync(row.DistancesByWordsIds, NpgsqlDbType.Bytea, ct);
            }

            await importer.CompleteAsync(ct);
        }

        await AttachPartition(WordsDistanceMapsTableName, partitionName, version, ct);
    }

    private static string GetPartitionName(string tableName, int version)
    {
        if (version < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Words version must be non-negative.");
        }

        return $"{tableName}_v{version}";
    }

    private async Task AttachPartition(
        string tableName,
        string partitionName,
        int version,
        CancellationToken ct)
    {
        await Execute(
            $"""
            ALTER TABLE {SchemaName}.{tableName}
            ATTACH PARTITION {SchemaName}.{partitionName}
            FOR VALUES IN ({version})
            """,
            ct);
    }

    private async Task DetachPartitionIfAttached(
        string tableName,
        string partitionName,
        CancellationToken ct)
    {
        var attached = await IsPartitionAttached(tableName, partitionName, ct);
        if (!attached) return;

        await DetachPartition(tableName, partitionName, ct);
    }

    private Task DropTableIfEsixts(
        string tableName,
        CancellationToken ct)
    {
        return Execute(
             $"DROP TABLE IF EXISTS {SchemaName}.{tableName}",
             ct);
    }

    private async Task CreateTableFromParent(
        string tableName,
        string partitionName,
        int version,
        CancellationToken ct)
    {
        await DropTableIfEsixts(partitionName, ct);

        await Execute(
            $"""
            CREATE TABLE {SchemaName}.{partitionName}
            (
                LIKE {SchemaName}.{tableName},

                CONSTRAINT pk_{partitionName}
                    PRIMARY KEY (version, word_id),

                CONSTRAINT ck_{partitionName}_version
                    CHECK (version = {version})
            )
            """,
            ct);
    }

    private async Task<bool> IsPartitionAttached(
        string tableName,
        string partitionName,
        CancellationToken ct)
    {

        var result = await ExecuteScalar<bool>(
            $"""
            SELECT EXISTS (
                SELECT 1
                FROM pg_inherits
                WHERE inhparent = '{SchemaName}.{tableName}'::regclass
                  AND inhrelid = to_regclass(@partitionName)
            )
            """,
            ct,
            parameters: [new NpgsqlParameter<string>("partitionName", $"{SchemaName}.{partitionName}")]);

        return result;
    }

    private Task DetachPartition(
        string tableName,
        string partitionName,
        CancellationToken ct)
    {
        return Execute(
           $"ALTER TABLE {SchemaName}.{tableName} DETACH PARTITION {SchemaName}.{partitionName} CONCURRENTLY",
           ct); ;
    }

    private async Task Execute(
        string sql,
        CancellationToken ct,
        NpgsqlTransaction? transaction = null,
        params NpgsqlParameter[] parameters)
    {
        await OpenConnection(ct);
        await using var command = new NpgsqlCommand(sql, _connection, transaction);
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(ct);
    }

    private async Task<T?> ExecuteScalar<T>(
        string sql,
        CancellationToken ct,
        NpgsqlTransaction? transaction = null,
        params NpgsqlParameter[] parameters)
    {
        await OpenConnection(ct);
        await using var command = new NpgsqlCommand(sql, _connection, transaction);
        command.Parameters.AddRange(parameters);
        var result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? default : (T)result;
    }

    private async Task OpenConnection(CancellationToken ct)
    {
        if (_connection.State == System.Data.ConnectionState.Closed)
        {
            await _connection.OpenAsync(ct);
        }
    }
}
