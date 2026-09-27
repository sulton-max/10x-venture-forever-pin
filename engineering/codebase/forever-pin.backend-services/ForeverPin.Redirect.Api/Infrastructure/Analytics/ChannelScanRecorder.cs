using System.Diagnostics.Metrics;
using System.Threading.Channels;
using ForeverPin.Redirect.Api.Application.Analytics.Models;
using ForeverPin.Redirect.Api.Application.Analytics.Services;

namespace ForeverPin.Redirect.Api.Infrastructure.Analytics;

/// <summary>Provides a bounded, observable in-memory scan queue.</summary>
public sealed class ChannelScanRecorder(ILogger<ChannelScanRecorder> logger) : IScanRecorder
{
    private static readonly Meter Meter = new("ForeverPin.Analytics");
    private static readonly Counter<long> DroppedScans = Meter.CreateCounter<long>("foreverpin.scans.dropped");
    private static readonly Counter<long> FailedScans = Meter.CreateCounter<long>("foreverpin.scans.failed");
    private static readonly Counter<long> PersistedScans = Meter.CreateCounter<long>("foreverpin.scans.persisted");
    private readonly Channel<ScanRecordModel> _channel = Channel.CreateBounded<ScanRecordModel>(
        new BoundedChannelOptions(capacity: 10_000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    private long _droppedCount;
    private long _failedCount;
    private long _persistedCount;

    /// <summary>Gets the reader over the queued scans.</summary>
    public ChannelReader<ScanRecordModel> Reader => _channel.Reader;

    /// <summary>Gets scans rejected because the queue was full or closed.</summary>
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    /// <summary>Gets accepted scans whose commit was not confirmed after a flush or shutdown failure.</summary>
    public long FailedCount => Interlocked.Read(ref _failedCount);

    /// <summary>Gets scans committed with their denormalized counters.</summary>
    public long PersistedCount => Interlocked.Read(ref _persistedCount);

    /// <inheritdoc />
    public void Enqueue(ScanRecordModel record)
    {
        if (_channel.Writer.TryWrite(record))
            return;

        DroppedScans.Add(1);
        var count = Interlocked.Increment(ref _droppedCount);
        if ((count & (count - 1)) == 0)
            logger.LogWarning("Scan queue rejected records; total rejected {Count}", count);
    }

    /// <summary>Stops accepting scans while allowing the worker to drain accepted records.</summary>
    public void Complete() => _channel.Writer.TryComplete();

    /// <summary>Records scans committed by the flush worker.</summary>
    public void RecordPersisted(int count)
    {
        Interlocked.Add(ref _persistedCount, count);
        PersistedScans.Add(count);
    }

    /// <summary>Records accepted scans that could not be persisted.</summary>
    public void RecordFailed(int count)
    {
        Interlocked.Add(ref _failedCount, count);
        FailedScans.Add(count);
    }
}
