namespace FolderSync.Tests;

public class FolderSynchronizerTests : IDisposable
{
    private readonly string _root;
    private readonly string _source;
    private readonly string _replica;
    private readonly string _logPath;
    private readonly FolderSynchronizer _synchronizer;

    public FolderSynchronizerTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "FolderSyncTests_" + Guid.NewGuid());
        _source = Path.Combine(_root, "source");
        _replica = Path.Combine(_root, "replica");
        _logPath = Path.Combine(_root, "logs", "sync.log");
        Directory.CreateDirectory(_source);

        _synchronizer = new FolderSynchronizer(_source, _replica, new Logger(_logPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private void CreateFile(string root, string relativePath, string content)
    {
        string path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static List<string> GetRelativeEntries(string root)
    {
        return Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path)
            .ToList();
    }

    private void AssertReplicaMatchesSource()
    {
        List<string> sourceEntries = GetRelativeEntries(_source);
        Assert.Equal(sourceEntries, GetRelativeEntries(_replica));

        foreach (string entry in sourceEntries)
        {
            string sourcePath = Path.Combine(_source, entry);
            if (File.Exists(sourcePath))
            {
                Assert.Equal(File.ReadAllText(sourcePath), File.ReadAllText(Path.Combine(_replica, entry)));
            }
        }
    }

    [Fact]
    public void Sync_ReplicaDoesNotExist_CreatesItWithFiles()
    {
        CreateFile(_source, "a.txt", "hello");
        
        _synchronizer.Sync();
        
        AssertReplicaMatchesSource();
    }

    [Fact]
    public void Sync_CopiesNestedFilesAndEmptyFolders()
    {
        CreateFile(_source, "a.txt", "a");
        CreateFile(_source, Path.Combine("sub", "deep", "b.txt"), "b");
        Directory.CreateDirectory(Path.Combine(_source, "empty"));

        _synchronizer.Sync();

        AssertReplicaMatchesSource();
    }

    [Fact]
    public void Sync_LogsCopiedFile()
    {
        CreateFile(_source, "a.txt", "a");

        _synchronizer.Sync();

        Assert.Contains("Copied file", File.ReadAllText(_logPath));
    }

    [Fact]
    public void Sync_SourceDoesNotExist_Throws()
    {
        Directory.Delete(_source);

        Assert.Throws<DirectoryNotFoundException>(() => _synchronizer.Sync());
    }
}