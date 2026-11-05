using FolderSync;

SyncOptions options;
try
{
    options = SyncOptions.Parse(args);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Usage: FolderSync <source> <replica> <intervalSeconds> <logFile>");
    return 1;
}

var logger = new Logger(options.LogPath);
var synchronizer = new FolderSynchronizer(options.SourcePath, options.ReplicaPath, logger);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    // do not kill the process in the middle of copying a file
    e.Cancel = true;
    cts.Cancel();
};

logger.Log($"Synchronization started. Source: {options.SourcePath}, replica: {options.ReplicaPath}, interval: {options.IntervalSeconds}s");
logger.Log("Press Ctrl+C to stop.");

while (!cts.IsCancellationRequested)
{
    try
    {
        synchronizer.Sync();
    }
    catch (Exception ex)
    {
        // a failed run should not stop the program, next run may succeed
        logger.Log($"Synchronization failed: {ex.Message}");
    }

    // wait for the next run, Ctrl+C interrupts the waiting
    cts.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(options.IntervalSeconds));
}

logger.Log("Synchronization stopped.");
return 0;