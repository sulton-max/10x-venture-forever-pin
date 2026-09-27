using ForeverPin.Redirect.Api.Application.Analytics.Models;

namespace ForeverPin.Redirect.Api.Infrastructure.Analytics.BackgroundServices;

/// <summary>Runs scan flushes as records arrive and drains accepted records during graceful shutdown.</summary>
public sealed class ScanFlushBackgroundService(
    ChannelScanRecorder recorder,
    IServiceScopeFactory scopeFactory,
    ILogger<ScanFlushBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 100;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = recorder.Reader;
        var buffer = new List<ScanRecordModel>(BatchSize);

        try
        {
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                while (buffer.Count < BatchSize && reader.TryRead(out var record))
                    buffer.Add(record);

                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var writer = scope.ServiceProvider.GetRequiredService<ScanBatchWriterService>();
                    await writer.WriteAsync(buffer, stoppingToken);
                    recorder.RecordPersisted(buffer.Count);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    recorder.RecordFailed(buffer.Count);
                    logger.LogError(ex, "Scan flush batch failed; {Count} records dropped", buffer.Count);
                }

                buffer.Clear();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host's shutdown budget expired before the accepted queue could drain.
        }
        finally
        {
            recorder.Complete();
            var lost = buffer.Count;
            while (reader.TryRead(out _))
                lost++;

            if (lost > 0)
            {
                recorder.RecordFailed(lost);
                logger.LogWarning("Scan shutdown left {Count} records without a confirmed commit", lost);
            }
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        recorder.Complete();
        if (ExecuteTask is not null)
        {
            try
            {
                await ExecuteTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Base cancellation interrupts an in-flight write when the drain budget expires.
            }
        }

        await base.StopAsync(cancellationToken);
    }
}
