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

        return new SyncOptions
        {
            SourcePath = Path.GetFullPath(args[0]),
            ReplicaPath = Path.GetFullPath(args[1]),
            IntervalSeconds = interval,
            LogPath = Path.GetFullPath(args[3])
        };
    }
}