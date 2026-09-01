using Xunit;

// The hand-rolled argparse-compatible parser. Every program's entire input goes
// through it, and since no program prompts any more it is the only way in.
public class ArgsTests
{
    private static ArgVals Parse(params string[] argv)
        => new Args("probe", "test")
            .Opt("-s", "--speed", @default: "1.0", help: "Speed")
            .Flag("-d", "--delete", help: "Delete")
            .Rest("files", min: 0, help: "Files")
            .Parse(argv);

    [Fact]
    public void ReadsASeparatedValue()
        => Assert.Equal("1.5", Parse("-s", "1.5").Str("speed"));

    [Fact]
    public void ReadsAnAttachedShortValue()
        => Assert.Equal("1.5", Parse("-s1.5").Str("speed"));

    [Fact]
    public void ReadsAnEqualsForm()
        => Assert.Equal("1.5", Parse("--speed=1.5").Str("speed"));

    [Fact]
    public void FallsBackToTheDefault()
        => Assert.Equal("1.0", Parse().Str("speed"));

    [Fact]
    public void SetsFlagsOnlyWhenPresent()
    {
        Assert.False(Parse().Flag("delete"));
        Assert.True(Parse("-d").Flag("delete"));
        Assert.True(Parse("--delete").Flag("delete"));
    }

    [Fact]
    public void CollectsPositionalsInOrder()
    {
        var a = Parse("one.mkv", "two.mkv", "three.mkv");
        Assert.Equal(["one.mkv", "two.mkv", "three.mkv"], a.Rest);
    }

    [Fact]
    public void AllowsOptionsAndFilesToInterleave()
    {
        var a = Parse("one.mkv", "-s", "2.0", "two.mkv", "-d");
        Assert.Equal("2.0", a.Str("speed"));
        Assert.True(a.Flag("delete"));
        Assert.Equal(["one.mkv", "two.mkv"], a.Rest);
    }

    [Fact]
    public void TreatsEverythingAfterADoubleDashAsAFile()
    {
        // The reason the shebang passes -- before the program's own arguments.
        var a = Parse("--", "-s", "weird-name.mkv");
        Assert.Equal("1.0", a.Str("speed"));
        Assert.Contains("-s", a.Rest);
    }

    [Fact]
    public void KeepsAFilenameThatLooksLikeAnOption()
    {
        var a = Parse("--", "--delete");
        Assert.False(a.Flag("delete"));
        Assert.Contains("--delete", a.Rest);
    }

    [Fact]
    public void ParsesNumbersThroughTheInvariantCulture()
    {
        // A comma-decimal locale must not turn 1.5 into 15.
        Assert.Equal(1.5, Parse("-s", "1.5").Dbl("speed"), 3);
    }
}
