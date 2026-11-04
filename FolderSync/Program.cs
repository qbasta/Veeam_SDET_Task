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

Console.WriteLine($"Source: {options.SourcePath}");
Console.WriteLine($"Replica: {options.ReplicaPath}");
Console.WriteLine($"Interval: {options.IntervalSeconds}s");
Console.WriteLine($"Log file: {options.LogPath}");
return 0;