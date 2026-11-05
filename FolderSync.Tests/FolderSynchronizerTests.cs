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
    public void Sync_UpdatesFileWhenSizeChanged()
    {
        CreateFile(_source, "a.txt", "version 1");
        _synchronizer.Sync();

        CreateFile(_source, "a.txt", "version 2 is longer");
        _synchronizer.Sync();

        AssertReplicaMatchesSource();
        Assert.Contains("Updated file", File.ReadAllText(_logPath));
    }
    
    [Fact]
    public void Sync_UpdatesFileWhenContentChangedButSizeIsTheSame()
    {
        CreateFile(_source, "a.txt", "aaaa");
        _synchronizer.Sync();

        CreateFile(_source, "a.txt", "bbbb");
        _synchronizer.Sync();

        AssertReplicaMatchesSource();
    }

    [Fact]
    public void Sync_NothingChanged_DoesNotLogAnything()
    {
        CreateFile(_source, "a.txt", "a");
        _synchronizer.Sync();
        int linesAfterFirstSync = File.ReadAllLines(_logPath).Length;

        _synchronizer.Sync();

        Assert.Equal(linesAfterFirstSync, File.ReadAllLines(_logPath).Length);
    }
    
    [Fact]
    public void Sync_FileInReplicaHasTheSameNameAsFolderInSource()
    {
        CreateFile(_source, Path.Combine("item", "inner.txt"), "inner");
        CreateFile(_replica, "item", "I am a file");

        _synchronizer.Sync();

        AssertReplicaMatchesSource();
    }

    [Fact]
    public void Sync_FolderInReplicaHasTheSameNameAsFileInSource()
    {
        CreateFile(_source, "item", "I am a file");
        CreateFile(_replica, Path.Combine("item", "inner.txt"), "inner");

        _synchronizer.Sync();

        AssertReplicaMatchesSource();
    }
    
    [Fact]
    public void Sync_SourceDoesNotExist_Throws()
    {
        Directory.Delete(_source);

        Assert.Throws<DirectoryNotFoundException>(() => _synchronizer.Sync());
    }
    
    [Fact]
    public void Sync_RemovesFilesMissingInSource()
    {
        CreateFile(_source, "keep.txt", "keep");
        CreateFile(_replica, "keep.txt", "keep");
        CreateFile(_replica, "extra.txt", "extra");

        _synchronizer.Sync();

        AssertReplicaMatchesSource();
    }

    [Fact]
    public void Sync_RemovesFoldersMissingInSource()
    {
        CreateFile(_source, "a.txt", "a");
        CreateFile(_replica, Path.Combine("old", "deep", "f.txt"), "old");

        _synchronizer.Sync();

        AssertReplicaMatchesSource();
        Assert.False(Directory.Exists(Path.Combine(_replica, "old")));
    }

    [Fact]
    public void Sync_RemovesFileThatWasDeletedFromSource()
    {
        CreateFile(_source, "a.txt", "a");
        CreateFile(_source, "b.txt", "b");
        _synchronizer.Sync();

        File.Delete(Path.Combine(_source, "b.txt"));
        _synchronizer.Sync();

        AssertReplicaMatchesSource();
        Assert.Contains("Deleted file", File.ReadAllText(_logPath));
    }
}