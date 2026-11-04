namespace FolderSync;

public class Logger
{
    private readonly string _logPath;

    public Logger(string logPath)
    {
        _logPath = logPath;

        string? directory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Log(string message)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
        Console.WriteLine(line);
        File.AppendAllText(_logPath, line + Environment.NewLine);
    }
}