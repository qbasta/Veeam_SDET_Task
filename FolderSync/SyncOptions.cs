namespace FolderSync;

public class SyncOptions
{
    public string SourcePath { get; set; } = "";
    public string ReplicaPath { get; set; } = "";
    public int IntervalSeconds { get; set; }
    public string LogPath { get; set; } = "";

    public static SyncOptions Parse(string[] args)
    {
        if (args.Length != 4)
        {
            throw new ArgumentException("Expected 4 arguments.");
        }

        if (!int.TryParse(args[2], out int interval) || interval <= 0)
        {
            throw new ArgumentException("Interval must be a positive number of seconds.");
        }

        string source = Path.GetFullPath(args[0]);
        string replica = Path.GetFullPath(args[1]);
        string log = Path.GetFullPath(args[3]);
        
        // syncing a folder into itself would never end
        if (IsInside(source, replica) || IsInside(replica, source))
        {
            throw new ArgumentException("Source and replica folders cannot be the same or nested in each other.");
        }

        // the log file would be copied to the replica and changed by every run
        if (IsInside(log, source))
        {
            throw new ArgumentException("Log file cannot be placed inside the source folder.");
        }

        // the log file would be deleted as a file that does not exist in source
        if (IsInside(log, replica))
        {
            throw new ArgumentException("Log file cannot be placed inside the replica folder.");
        }

        return new SyncOptions
        {
            SourcePath = source,
            ReplicaPath = replica,
            IntervalSeconds = interval,
            LogPath = log
        };
    }
    
    // true if path is the same as folder or is located somewhere inside of it
    private static bool IsInside(string path, string folder)
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string normalizedPath = Path.TrimEndingDirectorySeparator(path) + separator;
        string normalizedFolder = Path.TrimEndingDirectorySeparator(folder) + separator;

        return normalizedPath.StartsWith(normalizedFolder, StringComparison.Ordinal);
    }
}