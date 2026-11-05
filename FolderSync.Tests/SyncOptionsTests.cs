using Xunit;

namespace FolderSync.Tests;

public class SyncOptionsTests
{
    [Fact]
    public void Parse_ValidArguments_FillsOptions()
    {
        var options = SyncOptions.Parse(new[] { "src", "replica", "10", "sync.log" });

        Assert.Equal(Path.GetFullPath("src"), options.SourcePath);
        Assert.Equal(Path.GetFullPath("replica"), options.ReplicaPath);
        Assert.Equal(10, options.IntervalSeconds);
        Assert.Equal(Path.GetFullPath("sync.log"), options.LogPath);
    }

    [Fact]
    public void Parse_WrongNumberOfArguments_Throws()
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { "src", "replica" }));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1.5")]
    public void Parse_InvalidInterval_Throws(string interval)
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { "src", "replica", interval, "sync.log" }));
    }
    
    [Fact]
    public void Parse_ReplicaInsideSource_Throws()
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { "data", Path.Combine("data", "replica"), "10", "sync.log" }));
    }

    [Fact]
    public void Parse_SourceInsideReplica_Throws()
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { Path.Combine("backup", "data"), "backup", "10", "sync.log" }));
    }

    [Fact]
    public void Parse_SameSourceAndReplica_Throws()
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { "data", "data", "10", "sync.log" }));
    }

    [Fact]
    public void Parse_FoldersWithSimilarNames_AreAllowed()
    {
        var options = SyncOptions.Parse(new[] { "data", "data-backup", "10", "sync.log" });

        Assert.Equal(Path.GetFullPath("data-backup"), options.ReplicaPath);
    }

    [Fact]
    public void Parse_LogInsideReplica_Throws()
    {
        Assert.Throws<ArgumentException>(() => SyncOptions.Parse(new[] { "src", "replica", "10", Path.Combine("replica", "sync.log") }));
    }
}