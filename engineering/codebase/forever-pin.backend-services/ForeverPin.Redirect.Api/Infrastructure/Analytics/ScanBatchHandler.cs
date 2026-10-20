using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Analytics.Models;
using Microsoft.EntityFrameworkCore;
using WoW.Two.Sdk.Beta.Be.Jobs.Batches;

namespace ForeverPin.Redirect.Api.Infrastructure.Analytics;

/// <summary>Commits a scan batch and every affected counter in one database transaction.</summary>
public sealed class ScanBatchHandler(AppDbContext db) : IBatchHandler<ScanRecordModel>
{
    /// <summary>Persists all records and their counter increments atomically.</summary>
    public async Task HandleAsync(IReadOnlyList<ScanRecordModel> records, CancellationToken ct)
    {
        if (records.Count == 0)
            return;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        foreach (var record in records)
        {
            db.ScanEvents.Add(new ScanEventEntity
            {
                Id = Guid.NewGuid(),
                CodeId = record.CodeId,
                ScannedAt = record.ScannedAt,
                Device = record.Device,
                CountryCode = record.CountryCode,
                Os = record.Os,
                Referrer = record.Referrer,
                UserAgentHash = record.UserAgentHash,
                MatchedRuleOrder = record.MatchedRuleOrder,
                DestinationUrl = record.DestinationUrl,
            });
        }

        await db.SaveChangesAsync(ct);
        foreach (var group in records.GroupBy(record => record.CodeId).OrderBy(group => group.Key))
        {
            var increment = group.Count();
            await db.Codes
                .Where(code => code.Id == group.Key)
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    code => code.ScanCount, code => code.ScanCount + increment), ct);
        }

        await transaction.CommitAsync(ct);
    }
}
