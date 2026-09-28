using System.Data.Common;
using ForeverPin.Application.Codes.Analytics.Models;
using ForeverPin.Application.Codes.Analytics.Services;
using ForeverPin.Persistence.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace ForeverPin.Infrastructure.Persistence.Repositories;

/// <summary>Groups recorded scans in PostgreSQL over the <c>(code_id, scanned_at)</c> index.</summary>
public sealed class ScanAnalyticsRepository(AppDbContext db) : IScanAnalyticsRepository
{
    /// <inheritdoc />
    public async Task<ScanTallyModel> TallyAsync(Guid codeId, DateTimeOffset since, DateTimeOffset until, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            return new ScanTallyModel
            {
                Days = await GroupAsync("(scanned_at AT TIME ZONE 'UTC')::date", "1",
                    reader => new ScanDayModel(reader.GetFieldValue<DateOnly>(0), reader.GetInt64(1))),
                Devices = await GroupAsync("device", "2 DESC, 1", Share),
                OperatingSystems = await GroupAsync("os", "2 DESC, 1", Share),
                Countries = await GroupAsync("country_code", "2 DESC, 1", Share),
            };
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        // The key and order are the fixed fragments above, never caller input.
        async Task<IReadOnlyList<T>> GroupAsync<T>(string key, string order, Func<DbDataReader, T> read)
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"""
                SELECT {key}, count(*) FROM scan_events
                WHERE code_id = @code AND scanned_at >= @since AND scanned_at < @until
                GROUP BY 1 ORDER BY {order}
                """;
            AddParameter(command, "code", codeId);
            AddParameter(command, "since", since.ToUniversalTime());
            AddParameter(command, "until", until.ToUniversalTime());

            var rows = new List<T>();
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                rows.Add(read(reader));
            return rows;
        }
    }

    private static ScanShareModel Share(DbDataReader reader) =>
        new(reader.IsDBNull(0) ? null : reader.GetString(0), reader.GetInt64(1));

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
