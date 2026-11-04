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
}