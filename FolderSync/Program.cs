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
logger.Log($"Source: {options.SourcePath}");
logger.Log($"Replica: {options.ReplicaPath}");
logger.Log($"Interval: {options.IntervalSeconds}s");

var synchronizer = new FolderSynchronizer(options.SourcePath, options.ReplicaPath, logger);
synchronizer.Sync();
return 0;