namespace FolderSync.Tests;

public class LoggerTests : IDisposable
{
    private readonly string _folder;

    public LoggerTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "FolderSyncTests_" + Guid.NewGuid());
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    [Fact]
    public void Log_WritesMessageToFile()
    {
        string logPath = Path.Combine(_folder, "sync.log");
        var logger = new Logger(logPath);

        logger.Log("first message");
        logger.Log("second message");

        string[] lines = File.ReadAllLines(logPath);
        Assert.Equal(2, lines.Length);
        Assert.Contains("first message", lines[0]);
        Assert.Contains("second message", lines[1]);
    }

    [Fact]
    public void Log_WritesMessageToConsole()
    {
        var logger = new Logger(Path.Combine(_folder, "sync.log"));
        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            logger.Log("hello console");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Contains("hello console", writer.ToString());
    }
}